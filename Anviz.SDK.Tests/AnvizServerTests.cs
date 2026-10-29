using Anviz.SDK.Events;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Anviz.SDK.Tests
{
    public class AnvizServerTests
    {
        private static readonly TimeSpan WAIT = TimeSpan.FromSeconds(5);

        /// <summary>
        /// A running server listening on a free port of localhost, recording its events
        /// </summary>
        private sealed class Running : IAsyncDisposable
        {
            public AnvizServer Server { get; }
            public Task Run { get; }
            public ConcurrentQueue<DeviceConnectedEventArgs> Connected { get; } = new();
            public ConcurrentQueue<DeviceDisconnectedEventArgs> Disconnected { get; } = new();
            public ConcurrentQueue<AnvizServerErrorEventArgs> Errors { get; } = new();
            public ConcurrentQueue<DeviceRecordEventArgs> Records { get; } = new();
            public ConcurrentQueue<string> Order { get; } = new();
            private readonly CancellationTokenSource cts = new();

            public int Port => Server.LocalEndpoints[0].Port;

            public Running(Action<AnvizServer>? setup = null, bool listen = true)
            {
                Server = new AnvizServer { ReconnectDelay = TimeSpan.FromMilliseconds(100), HandshakeTimeout = TimeSpan.FromSeconds(2) };
                Server.DeviceConnected += (s, e) => { Connected.Enqueue(e); Order.Enqueue("connected " + e.Serial); };
                Server.DeviceDisconnected += (s, e) => { Disconnected.Enqueue(e); Order.Enqueue($"disconnected {e.Serial} {e.Reason}"); };
                Server.Error += (s, e) => Errors.Enqueue(e);
                Server.RecordReceived += (s, e) => Records.Enqueue(e);
                setup?.Invoke(Server);
                if (listen)
                {
                    Server.Listen(new IPEndPoint(IPAddress.Loopback, 0));
                }
                Run = Server.RunAsync(cts.Token);
            }

            public async Task Stop()
            {
                cts.Cancel();
                await Run.WaitAsync(WAIT);
            }

            public async ValueTask DisposeAsync()
            {
                cts.Cancel();
                try
                {
                    await Run.WaitAsync(WAIT);
                }
                finally
                {
                    Server.Dispose();
                }
            }
        }

        private static async Task WaitUntil(Func<bool> condition, TimeSpan? timeout = null)
        {
            var until = DateTime.UtcNow + (timeout ?? WAIT);
            while (!condition())
            {
                Assert.True(DateTime.UtcNow < until, "condition not met in time");
                await Task.Delay(10);
            }
        }

        [Fact]
        public async Task IncomingDeviceIsKnownBySerial()
        {
            await using var running = new Running();
            await using var fake = FakeDevice.ConnectTo(running.Port, "IN0001");

            await WaitUntil(() => running.Connected.Count == 1);

            var connected = running.Connected.Single();
            Assert.Equal("IN0001", connected.Serial);
            Assert.Equal(DeviceOrigin.Incoming, connected.Origin);
            Assert.True(running.Server.TryGetDevice("IN0001", out var device));
            Assert.Equal("IN0001", await device.GetDeviceSN());
        }

        [Fact]
        public async Task OutgoingDeviceIsConnectedAgainWhenLost()
        {
            await using var fake = new FakeDevice { Serial = "OUT001" };
            await using var running = new Running(s => s.AddDevice("127.0.0.1", fake.Port), listen: false);

            await WaitUntil(() => running.Connected.Count == 1);
            Assert.Equal(DeviceOrigin.Outgoing, running.Connected.Single().Origin);

            fake.Disconnect();

            await WaitUntil(() => running.Connected.Count == 2);
            Assert.Equal(DisconnectReason.Lost, running.Disconnected.Single().Reason);
            Assert.Equal(new[] { "connected OUT001", "disconnected OUT001 Lost", "connected OUT001" }, running.Order);
            Assert.True(running.Server.TryGetDevice("OUT001", out var device));
            Assert.Equal("OUT001", await device.GetDeviceSN());
        }

        [Fact]
        public async Task ListensAndConnectsAtTheSameTime()
        {
            await using var outgoing = new FakeDevice { Serial = "OUT002" };
            await using var running = new Running(s => s.AddDevice("127.0.0.1", outgoing.Port));
            await using var incoming = FakeDevice.ConnectTo(running.Port, "IN0002");

            await WaitUntil(() => running.Connected.Count == 2);

            Assert.Equal(new[] { "IN0002", "OUT002" }, running.Server.Serials.OrderBy(s => s));
            Assert.Equal(DeviceOrigin.Incoming, running.Connected.Single(c => c.Serial == "IN0002").Origin);
            Assert.Equal(DeviceOrigin.Outgoing, running.Connected.Single(c => c.Serial == "OUT002").Origin);
        }

        [Fact]
        public async Task NewConnectionOfTheSameSerialReplacesTheOld()
        {
            await using var running = new Running();
            await using var first = FakeDevice.ConnectTo(running.Port, "SAME01");
            await WaitUntil(() => running.Connected.Count == 1);

            await using var second = FakeDevice.ConnectTo(running.Port, "SAME01");
            await WaitUntil(() => running.Connected.Count == 2);

            Assert.Equal(DisconnectReason.Replaced, running.Disconnected.Single().Reason);
            Assert.Equal(new[] { "SAME01" }, running.Server.Serials);
            Assert.True(running.Server.TryGetDevice("SAME01", out var device));
            Assert.Same(running.Connected.Last().Device, device);
            Assert.True(device.Connected);
        }

        [Fact]
        public async Task OutgoingDoesNotTakeThePlaceOfTheSameDeviceConnectedByItself()
        {
            // the same device (same serial) both listening and connecting by itself
            await using var listening = new FakeDevice { Serial = "BOTH01" };
            await using var running = new Running(s => s.AddDevice("127.0.0.1", listening.Port));
            await WaitUntil(() => running.Connected.Count == 1);

            await using var connecting = FakeDevice.ConnectTo(running.Port, "BOTH01");
            await WaitUntil(() => running.Connected.Count == 2);
            await Task.Delay(500); // several reconnect delays

            Assert.Equal(2, running.Connected.Count);
            Assert.Equal(DeviceOrigin.Incoming, running.Connected.Last().Origin);
            Assert.Equal(1, listening.Connections);
        }

        [Fact]
        public async Task HttpRequestIsDiscardedAndDoesNotStopTheServer()
        {
            await using var running = new Running();
            using (var bot = new TcpClient())
            {
                await bot.ConnectAsync(IPAddress.Loopback, running.Port);
                await bot.GetStream().WriteAsync(Encoding.ASCII.GetBytes("GET / HTTP/1.1\r\nHost: x\r\n\r\n"));
                await WaitUntil(() => !running.Errors.IsEmpty);
                // the server sent its handshake command, then closed it: with a reset on Windows,
                // since the rest of the request was not read
                var buffer = new byte[1024];
                try
                {
                    while (await bot.GetStream().ReadAsync(buffer).AsTask().WaitAsync(WAIT) > 0)
                    {
                    }
                }
                catch (IOException e) when (e.InnerException is SocketException { SocketErrorCode: SocketError.ConnectionReset })
                {
                }
            }
            Assert.Empty(running.Connected);
            Assert.NotNull(running.Errors.First().EndPoint);

            await using var fake = FakeDevice.ConnectTo(running.Port, "AFTER1");
            await WaitUntil(() => running.Connected.Count == 1);
        }

        [Fact]
        public async Task SilentConnectionTimesOutWithoutBlockingTheOthers()
        {
            await using var running = new Running(s => s.HandshakeTimeout = TimeSpan.FromMilliseconds(800));
            using var silent = new TcpClient();
            await silent.ConnectAsync(IPAddress.Loopback, running.Port);

            await using var fake = FakeDevice.ConnectTo(running.Port, "QUICK1");
            await WaitUntil(() => running.Connected.Count == 1, TimeSpan.FromMilliseconds(700));

            await WaitUntil(() => running.Errors.Any(e => e.Exception is TimeoutException));
            Assert.Single(running.Connected);
        }

        [Fact]
        public async Task OutgoingToSomethingElseIsDiscardedAndRetried()
        {
            var http = new TcpListener(IPAddress.Loopback, 0);
            http.Start();
            var port = ((IPEndPoint)http.LocalEndpoint).Port;
            var accepted = 0;
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    TcpClient peer;
                    try { peer = await http.AcceptTcpClientAsync(); } catch { return; }
                    Interlocked.Increment(ref accepted);
                    _ = Task.Run(async () =>
                    {
                        using (peer)
                        {
                            var stream = peer.GetStream();
                            Assert.True(await stream.ReadAsync(new byte[64]) > 0); // the handshake command
                            await stream.WriteAsync(Encoding.ASCII.GetBytes("HTTP/1.1 400 Bad Request\r\n\r\n"));
                            await Task.Delay(200);
                        }
                    });
                }
            });
            try
            {
                await using var running = new Running(s => s.AddDevice("127.0.0.1", port), listen: false);

                await WaitUntil(() => Volatile.Read(ref accepted) >= 2 && running.Errors.Count >= 2);

                Assert.Empty(running.Connected);
                Assert.Empty(running.Server.Serials);
            }
            finally
            {
                http.Stop();
            }
        }

        [Fact]
        public async Task UnreachableAddressIsReportedAndRetried()
        {
            var port = ConnectionTests.FreePort(); // nothing listens there
            await using var running = new Running(s => s.AddDevice("127.0.0.1", port), listen: false);

            await WaitUntil(() => running.Errors.Count >= 2);

            Assert.All(running.Errors, e => Assert.IsType<DnsEndPoint>(e.EndPoint));
            Assert.Empty(running.Connected);
        }

        [Fact]
        public async Task SilentDeviceIsDisconnectedAfterTheHeartbeatTimeout()
        {
            await using var running = new Running(s => s.HeartbeatTimeout = TimeSpan.FromMilliseconds(300));
            await using var quiet = FakeDevice.ConnectTo(running.Port, "QUIET1");
            await using var alive = FakeDevice.ConnectTo(running.Port, "ALIVE1");
            await WaitUntil(() => running.Connected.Count == 2);

            for (var i = 0; i < 12; i++)
            {
                await alive.Send(0x7F, Array.Empty<byte>());
                await Task.Delay(50);
            }

            var disconnected = Assert.Single(running.Disconnected);
            Assert.Equal("QUIET1", disconnected.Serial);
            Assert.Equal(DisconnectReason.HeartbeatTimeout, disconnected.Reason);
            Assert.Equal(new[] { "ALIVE1" }, running.Server.Serials);
        }

        [Fact]
        public async Task AcceptDeviceCanRefuseADevice()
        {
            await using var running = new Running(s => s.AcceptDevice = e => e.Serial.StartsWith("OK"));
            await using var refused = FakeDevice.ConnectTo(running.Port, "NO0001");
            await using var accepted = FakeDevice.ConnectTo(running.Port, "OK0001");

            await WaitUntil(() => running.Connected.Count == 1);
            await Task.Delay(200);

            Assert.Equal("OK0001", running.Connected.Single().Serial);
            Assert.Equal(new[] { "OK0001" }, running.Server.Serials);
        }

        [Fact]
        public async Task RecordsAreReportedWithTheSerial()
        {
            await using var running = new Running();
            await using var fake = FakeDevice.ConnectTo(running.Port, "REC001");
            await WaitUntil(() => running.Connected.Count == 1);

            await fake.Send(0xDF, new Responses.Record(42) { DateTime = new DateTime(2026, 10, 28, 9, 30, 0) }.ToArray());

            await WaitUntil(() => running.Records.Count == 1);
            var record = running.Records.Single();
            Assert.Equal("REC001", record.Serial);
            Assert.Equal(42UL, record.Record.UserCode);
        }

        [Fact]
        public async Task HandlerExceptionsGoToError()
        {
            await using var running = new Running(s => s.DeviceConnected += (o, e) => throw new InvalidOperationException("handler"));
            await using var fake = FakeDevice.ConnectTo(running.Port, "THROW1");

            await WaitUntil(() => running.Errors.Any(e => e.Exception is InvalidOperationException));

            Assert.Equal("THROW1", running.Errors.First(e => e.Exception is InvalidOperationException).Serial);
            Assert.True(running.Server.TryGetDevice("THROW1", out _));
        }

        [Fact]
        public async Task RemovedDeviceIsDisconnectedAndNotConnectedAgain()
        {
            await using var fake = new FakeDevice { Serial = "RM0001" };
            await using var running = new Running(s => s.AddDevice("127.0.0.1", fake.Port), listen: false);
            await WaitUntil(() => running.Connected.Count == 1);

            Assert.True(running.Server.RemoveDevice("127.0.0.1", fake.Port));
            await WaitUntil(() => running.Disconnected.Count == 1);
            await Task.Delay(400); // several reconnect delays

            Assert.Equal(DisconnectReason.Removed, running.Disconnected.Single().Reason);
            Assert.Single(running.Connected);
            Assert.Empty(running.Server.Serials);
        }

        [Fact]
        public async Task DisconnectClosesOneDeviceThatConnectsAgain()
        {
            await using var fake = new FakeDevice { Serial = "KICK01" };
            await using var running = new Running(s => s.AddDevice("127.0.0.1", fake.Port), listen: false);
            await WaitUntil(() => running.Connected.Count == 1);

            Assert.True(running.Server.Disconnect("KICK01"));
            Assert.False(running.Server.Disconnect("NOPE"));

            await WaitUntil(() => running.Connected.Count == 2);
            Assert.Equal(DisconnectReason.Closed, running.Disconnected.Single().Reason);
        }

        [Fact]
        public async Task DevicesListsTheConnectedDevices()
        {
            await using var running = new Running();
            await using var fake = FakeDevice.ConnectTo(running.Port, "LIST01");
            await WaitUntil(() => running.Connected.Count == 1);
            var before = running.Server.Devices.Single();

            await Task.Delay(50);
            await fake.Send(0x7F, Array.Empty<byte>());
            await WaitUntil(() => running.Server.Devices.Single().LastActivity > before.LastActivity);

            var device = running.Server.Devices.Single();
            Assert.Equal("LIST01", device.Serial);
            Assert.Equal(DeviceOrigin.Incoming, device.Origin);
            Assert.True(device.ConnectedAt <= device.LastActivity);
        }

        [Fact]
        public async Task StoppingDisconnectsEveryDevice()
        {
            await using var outgoing = new FakeDevice { Serial = "STOP01" };
            var running = new Running(s => s.AddDevice("127.0.0.1", outgoing.Port));
            await using var incoming = FakeDevice.ConnectTo(running.Port, "STOP02");
            await WaitUntil(() => running.Connected.Count == 2);

            await running.Stop();

            Assert.Equal(2, running.Disconnected.Count);
            Assert.All(running.Disconnected, d => Assert.Equal(DisconnectReason.Stopped, d.Reason));
            Assert.Empty(running.Server.Serials);
            Assert.Throws<ObjectDisposedException>(() => running.Server.Listen(0));
            await running.DisposeAsync();
        }
    }
}
