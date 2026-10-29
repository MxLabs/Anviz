using Anviz.SDK.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Anviz.SDK
{
    /// <summary>
    /// Keeps many devices connected and tells when they come and go, identified by serial number.
    /// It can listen for devices in client mode (<see cref="Listen(IPEndPoint)"/>), connect to devices
    /// in server mode and reconnect when they are lost (<see cref="AddDevice"/>), or both.
    /// A connection is a device only after its handshake: anything else (a scanner, an HTTP request,
    /// a wrong address) is closed and reported with <see cref="Error"/>.
    /// </summary>
    public class AnvizServer : IDisposable
    {
        private readonly object sync = new object();
        private readonly List<TcpListener> listeners = new List<TcpListener>();
        private readonly Dictionary<string, Target> targets = new Dictionary<string, Target>();
        private readonly ConcurrentDictionary<string, Connection> devices = new ConcurrentDictionary<string, Connection>();
        private readonly List<Task> tasks = new List<Task>();
        private readonly CancellationTokenSource stopping = new CancellationTokenSource();
        private int running;
        private int handshakes;

        public string ConnectionUser { get; set; } = "admin";
        public string ConnectionPassword { get; set; } = "12345";
        public bool AuthenticateConnection { get; set; } = false;

        /// <summary>
        /// How long a new connection has to connect (outgoing) and answer the handshake
        /// </summary>
        public TimeSpan HandshakeTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// At most this many connections in handshake at the same time: more incoming ones are
        /// closed right away, so a flood of connections cannot exhaust the server
        /// </summary>
        public int MaxHandshakes { get; set; } = 64;

        /// <summary>
        /// When set, a device that sends nothing (ping, record, answer to a command) for this long is
        /// disconnected. Devices in client mode ping regularly; one in server mode only answers, so
        /// keep it longer than the time between your commands
        /// </summary>
        public TimeSpan? HeartbeatTimeout { get; set; }

        /// <summary>
        /// How long to wait before connecting again to an address added with <see cref="AddDevice"/>
        /// </summary>
        public TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Decides whether a device that passed the handshake is kept: return false to close it.
        /// All devices are kept when it is not set
        /// </summary>
        public Func<DeviceConnectingEventArgs, bool> AcceptDevice { get; set; }

        // An exception thrown by a handler is reported with Error. An async handler (async void) that
        // throws after its first await cannot be caught by anyone and ends the process: catch its
        // exceptions inside it.

        public event EventHandler<DeviceConnectedEventArgs> DeviceConnected;
        public event EventHandler<DeviceDisconnectedEventArgs> DeviceDisconnected;
        public event EventHandler<DeviceRecordEventArgs> RecordReceived;
        public event EventHandler<AnvizServerErrorEventArgs> Error;

        /// <summary>
        /// The addresses the server listens on, with the actual port when it was 0
        /// </summary>
        public IReadOnlyList<IPEndPoint> LocalEndpoints
        {
            get
            {
                lock (sync)
                {
                    return listeners.Select(l => (IPEndPoint)l.LocalEndpoint).ToList();
                }
            }
        }

        /// <summary>
        /// The serial numbers of the connected devices
        /// </summary>
        public ICollection<string> Serials => devices.Keys;

        /// <summary>
        /// The connected devices, as they are now
        /// </summary>
        public IReadOnlyList<ConnectedDevice> Devices => devices.Values
            .Select(c => new ConnectedDevice(c.Serial, c.Device, c.EndPoint, c.Origin, c.ConnectedAt, c.LastActivity))
            .ToList();

        /// <summary>
        /// Closes the connection of a device, with <see cref="DisconnectReason.Closed"/>. A device in
        /// client mode connects again by itself, one added with <see cref="AddDevice"/> is connected
        /// again after <see cref="ReconnectDelay"/>: this is how to have it go through
        /// <see cref="AcceptDevice"/> again
        /// </summary>
        public bool Disconnect(string serial)
        {
            if (!devices.TryGetValue(serial, out var connection))
            {
                return false;
            }
            Close(connection, DisconnectReason.Closed);
            return true;
        }

        public bool TryGetDevice(string serial, out AnvizDevice device)
        {
            if (devices.TryGetValue(serial, out var connection))
            {
                device = connection.Device;
                return true;
            }
            device = null;
            return false;
        }

        public void Listen(int port = 5010)
        {
            Listen(new IPEndPoint(IPAddress.Any, port));
        }

        /// <summary>
        /// Listens for devices in client mode. It starts listening now; connections are taken
        /// once <see cref="RunAsync"/> runs
        /// </summary>
        public void Listen(IPEndPoint localEP)
        {
            var listener = new TcpListener(localEP);
            lock (sync)
            {
                CheckNotStopped();
                listener.Start();
                listeners.Add(listener);
                if (running == 1)
                {
                    Track(AcceptLoop(listener));
                }
            }
        }

        /// <summary>
        /// Connects to a device in server mode, now if the server runs or when it starts, and
        /// connects again whenever the connection is lost
        /// </summary>
        public void AddDevice(string host, int port = 5010)
        {
            var target = new Target(host, port);
            lock (sync)
            {
                CheckNotStopped();
                if (targets.ContainsKey(target.Key))
                {
                    return;
                }
                targets.Add(target.Key, target);
                if (running == 1)
                {
                    Track(TargetLoop(target));
                }
            }
        }

        /// <summary>
        /// Stops connecting to an address added with <see cref="AddDevice"/>, and closes its
        /// connection with <see cref="DisconnectReason.Removed"/>
        /// </summary>
        public bool RemoveDevice(string host, int port = 5010)
        {
            Target target;
            lock (sync)
            {
                if (!targets.TryGetValue(new Target(host, port).Key, out target))
                {
                    return false;
                }
                targets.Remove(target.Key);
            }
            target.Stop.Cancel();
            var current = target.Current;
            if (current != null)
            {
                Close(current, DisconnectReason.Removed);
            }
            return true;
        }

        /// <summary>
        /// Runs the server until the token is cancelled or the server is disposed, then closes
        /// every device with <see cref="DisconnectReason.Stopped"/>. It runs once
        /// </summary>
        public async Task RunAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            lock (sync)
            {
                CheckNotStopped();
                if (Interlocked.Exchange(ref running, 1) == 1)
                {
                    throw new InvalidOperationException("The server is already running");
                }
                foreach (var listener in listeners)
                {
                    Track(AcceptLoop(listener));
                }
                foreach (var target in targets.Values)
                {
                    Track(TargetLoop(target));
                }
                if (HeartbeatTimeout.HasValue)
                {
                    Track(HeartbeatLoop(HeartbeatTimeout.Value));
                }
            }
            using (cancellationToken.Register(() => stopping.Cancel()))
            {
                await Wait(Timeout.InfiniteTimeSpan, stopping.Token);
            }
            Stop();
            Task[] pending;
            lock (sync)
            {
                pending = tasks.ToArray();
            }
            try
            {
                await Task.WhenAll(pending);
            }
            catch (Exception)
            {
                // every loop reports its own errors
            }
        }

        public void Dispose()
        {
            stopping.Cancel();
            Stop();
        }

        private void Stop()
        {
            lock (sync)
            {
                foreach (var listener in listeners)
                {
                    listener.Stop();
                }
                foreach (var target in targets.Values)
                {
                    target.Stop.Cancel();
                }
            }
            foreach (var connection in devices.Values)
            {
                Close(connection, DisconnectReason.Stopped);
            }
        }

        private void CheckNotStopped()
        {
            if (stopping.IsCancellationRequested)
            {
                throw new ObjectDisposedException(nameof(AnvizServer), "The server stopped");
            }
        }

        private void Track(Task task)
        {
            lock (sync)
            {
                tasks.RemoveAll(t => t.IsCompleted);
                tasks.Add(task);
            }
        }

        private async Task AcceptLoop(TcpListener listener)
        {
            while (!stopping.IsCancellationRequested)
            {
                TcpClient tcp;
                try
                {
                    tcp = await listener.AcceptTcpClientAsync();
                }
                catch (Exception ex)
                {
                    if (!stopping.IsCancellationRequested)
                    {
                        RaiseError(listener.LocalEndpoint, null, ex);
                    }
                    return;
                }
                if (Interlocked.Increment(ref handshakes) > MaxHandshakes)
                {
                    Interlocked.Decrement(ref handshakes);
                    var endPoint = RemoteEndPoint(tcp);
                    tcp.Close();
                    RaiseError(endPoint, null, new Exception("Too many connections in handshake, closed"));
                    continue;
                }
                Track(Task.Run(async () =>
                {
                    try
                    {
                        await Handshake(tcp, RemoteEndPoint(tcp), DeviceOrigin.Incoming, null);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref handshakes);
                    }
                }));
            }
        }

        private async Task TargetLoop(Target target)
        {
            using (var stop = CancellationTokenSource.CreateLinkedTokenSource(stopping.Token, target.Stop.Token))
            {
                var token = stop.Token;
                while (!token.IsCancellationRequested)
                {
                    // connected in another way, e.g. it connects by itself too: do not take its place
                    var serial = target.Serial;
                    if (serial == null || !devices.TryGetValue(serial, out var other) || other.Target == target)
                    {
                        var connection = await ConnectTo(target, token);
                        if (connection != null)
                        {
                            await Wait(Timeout.InfiniteTimeSpan, token, connection.Closed.Task);
                        }
                    }
                    await Wait(ReconnectDelay, token);
                }
            }
        }

        private async Task<Connection> ConnectTo(Target target, CancellationToken token)
        {
            var tcp = new TcpClient();
            var endPoint = new DnsEndPoint(target.Host, target.Port);
            try
            {
                var connect = tcp.ConnectAsync(target.Host, target.Port);
                if (!await Wait(HandshakeTimeout, token, connect))
                {
                    Observe(connect);
                    tcp.Close();
                    if (!token.IsCancellationRequested)
                    {
                        RaiseError(endPoint, target.Serial, new TimeoutException("Connection timed out"));
                    }
                    return null;
                }
                await connect;
            }
            catch (Exception ex)
            {
                tcp.Close();
                RaiseError(endPoint, target.Serial, ex);
                return null;
            }
            if (token.IsCancellationRequested)
            {
                tcp.Close();
                return null;
            }
            return await Handshake(tcp, endPoint, DeviceOrigin.Outgoing, target);
        }

        /// <summary>
        /// Makes sure the peer is a device and registers it, or closes the connection
        /// </summary>
        private async Task<Connection> Handshake(TcpClient tcp, EndPoint endPoint, DeviceOrigin origin, Target target)
        {
            AnvizDevice device = null;
            try
            {
                device = new AnvizDevice(tcp);
                var handshake = Identify(device);
                if (!await Wait(HandshakeTimeout, stopping.Token, handshake))
                {
                    Observe(handshake);
                    throw stopping.IsCancellationRequested
                        ? (Exception)new OperationCanceledException()
                        : new TimeoutException("No valid answer to the handshake");
                }
                var serial = await handshake;
                if (string.IsNullOrEmpty(serial))
                {
                    throw new Exception("The device has no serial number");
                }
                var connecting = new DeviceConnectingEventArgs(serial, device, endPoint, origin);
                if (AcceptDevice != null && !AcceptDevice(connecting))
                {
                    device.Dispose();
                    return null;
                }
                return Register(new Connection(serial, device, endPoint, origin, target));
            }
            catch (Exception ex)
            {
                if (device != null)
                {
                    device.Dispose();
                }
                else
                {
                    tcp.Close();
                }
                if (!(ex is OperationCanceledException))
                {
                    RaiseError(endPoint, target?.Serial, ex);
                }
                return null;
            }
        }

        private async Task<string> Identify(AnvizDevice device)
        {
            if (AuthenticateConnection)
            {
                await device.SetConnectionPassword(ConnectionUser, ConnectionPassword);
            }
            await device.GetDeviceBiometricType();
            return await device.GetDeviceSN();
        }

        private Connection Register(Connection connection)
        {
            var device = connection.Device;
            device.Activity += (s, e) => connection.Touch();
            device.ReceivedRecord += (s, record) =>
            {
                Raise(RecordReceived, new DeviceRecordEventArgs(connection.Serial, device, record), connection);
            };
            Connection replaced = null;
            lock (sync)
            {
                if (stopping.IsCancellationRequested)
                {
                    device.Dispose();
                    return null;
                }
                devices.AddOrUpdate(connection.Serial, connection, (serial, old) =>
                {
                    replaced = old;
                    return connection;
                });
                if (connection.Target != null)
                {
                    connection.Target.Serial = connection.Serial;
                    connection.Target.Current = connection;
                }
            }
            if (replaced != null)
            {
                Close(replaced, DisconnectReason.Replaced);
            }
            Raise(DeviceConnected, new DeviceConnectedEventArgs(connection.Serial, device, connection.EndPoint, connection.Origin), connection);
            // subscribed after DeviceConnected, so that DeviceDisconnected always comes after it
            device.Disconnected += (s, e) => Close(connection, DisconnectReason.Lost);
            if (!device.Connected)
            {
                // lost before the handler was there
                Close(connection, DisconnectReason.Lost);
            }
            return connection;
        }

        /// <summary>
        /// Closes a connection once, with the first reason given
        /// </summary>
        private void Close(Connection connection, DisconnectReason reason)
        {
            if (Interlocked.Exchange(ref connection.ClosedFlag, 1) == 1)
            {
                return;
            }
            ((ICollection<KeyValuePair<string, Connection>>)devices).Remove(
                new KeyValuePair<string, Connection>(connection.Serial, connection));
            if (connection.Target != null && connection.Target.Current == connection)
            {
                connection.Target.Current = null;
            }
            connection.Device.Dispose();
            Raise(DeviceDisconnected, new DeviceDisconnectedEventArgs(connection.Serial, connection.EndPoint, connection.Origin, reason), connection);
            connection.Closed.TrySetResult(true);
        }

        private async Task HeartbeatLoop(TimeSpan timeout)
        {
            var period = TimeSpan.FromTicks(Math.Max(TimeSpan.FromMilliseconds(10).Ticks, Math.Min(timeout.Ticks / 4, TimeSpan.FromSeconds(1).Ticks)));
            while (!stopping.IsCancellationRequested)
            {
                await Wait(period, stopping.Token);
                foreach (var connection in devices.Values)
                {
                    if (connection.Idle > timeout)
                    {
                        Close(connection, DisconnectReason.HeartbeatTimeout);
                    }
                }
            }
        }

        private void Raise<T>(EventHandler<T> handler, T args, Connection connection)
        {
            try
            {
                handler?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                RaiseError(connection.EndPoint, connection.Serial, ex);
            }
        }

        private void RaiseError(EndPoint endPoint, string serial, Exception exception)
        {
            try
            {
                Error?.Invoke(this, new AnvizServerErrorEventArgs(endPoint, serial, exception));
            }
            catch (Exception)
            {
                // nowhere left to report it
            }
        }

        /// <summary>
        /// Waits for the delay, the token or the task, whichever comes first, without throwing:
        /// true when the task completed
        /// </summary>
        private static async Task<bool> Wait(TimeSpan delay, CancellationToken token, Task task = null)
        {
            using (var cancel = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                var wait = Task.Delay(delay, cancel.Token);
                var done = task == null ? await Task.WhenAny(wait) : await Task.WhenAny(task, wait);
                cancel.Cancel();
                Observe(wait);
                return task != null && done == task;
            }
        }

        private static EndPoint RemoteEndPoint(TcpClient tcp)
        {
            try
            {
                return tcp.Client.RemoteEndPoint;
            }
            catch (Exception)
            {
                // already closed by the peer
                return null;
            }
        }

        private static void Observe(Task task)
        {
            task.ContinueWith(t => { var _ = t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
        }

        private class Target
        {
            public string Host { get; }
            public int Port { get; }
            public string Key => Host.ToLowerInvariant() + ":" + Port;
            public CancellationTokenSource Stop { get; } = new CancellationTokenSource();
            public volatile string Serial;
            public volatile Connection Current;

            public Target(string host, int port)
            {
                Host = host;
                Port = port;
            }
        }

        private class Connection
        {
            public string Serial { get; }
            public AnvizDevice Device { get; }
            public EndPoint EndPoint { get; }
            public DeviceOrigin Origin { get; }
            public Target Target { get; }
            public TaskCompletionSource<bool> Closed { get; } =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            public int ClosedFlag;
            public DateTime ConnectedAt { get; } = DateTime.UtcNow;
            private long lastActivity = DateTime.UtcNow.Ticks;

            public DateTime LastActivity => new DateTime(Interlocked.Read(ref lastActivity), DateTimeKind.Utc);
            public TimeSpan Idle => DateTime.UtcNow - LastActivity;

            public Connection(string serial, AnvizDevice device, EndPoint endPoint, DeviceOrigin origin, Target target)
            {
                Serial = serial;
                Device = device;
                EndPoint = endPoint;
                Origin = origin;
                Target = target;
            }

            public void Touch()
            {
                Interlocked.Exchange(ref lastActivity, DateTime.UtcNow.Ticks);
            }
        }
    }
}
