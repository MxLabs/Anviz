using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Anviz.SDK
{
    internal class AnvizStream : IDisposable
    {
        private const int DEVICE_TIMEOUT = 20000; //20 seconds

        private readonly TcpClient DeviceSocket;
        private readonly NetworkStream DeviceStream;
        private readonly Thread ReceiverThread;
        private readonly CancellationTokenSource CancellationTokenSource;

        /// <summary>
        /// One command at a time: the device answers with the command code only, so two
        /// commands waiting at the same time could not be told apart
        /// </summary>
        private readonly SemaphoreSlim CommandLock = new SemaphoreSlim(1, 1);
        /// <summary>
        /// Commands and the pong of the receiver thread write to the same stream
        /// </summary>
        private readonly SemaphoreSlim WriteLock = new SemaphoreSlim(1, 1);
        private PendingCommand Pending;
        private bool Closed;
        /// <summary>
        /// A device in client mode pings every 10 seconds: a connection that was pinged and then
        /// says nothing for this long is lost, as when the cable is unplugged. Without a ping the
        /// TCP keepalive tells it
        /// </summary>
        internal TimeSpan PingSilence { get; set; } = TimeSpan.FromSeconds(30);
        private readonly Timer PingWatchdog;
        private long LastReceived = DateTime.UtcNow.Ticks;
        private bool Pinged;
        private int DisconnectedRaised;
        private int Disposed;

        private class PendingCommand
        {
            public byte ResponseCode { get; }
            public TaskCompletionSource<Response> Result { get; } =
                new TaskCompletionSource<Response>(TaskCreationOptions.RunContinuationsAsynchronously);

            public PendingCommand(byte responseCode)
            {
                ResponseCode = responseCode;
            }
        }

        public event EventHandler<Response> ReceivedPacket;
        public event EventHandler<Exception> DeviceError;
        public event EventHandler Disconnected;

        public bool Connected => !Volatile.Read(ref Closed);

        public AnvizStream(TcpClient socket)
        {
            CancellationTokenSource = new CancellationTokenSource();
            DeviceSocket = socket;
            DeviceStream = socket.GetStream();
            DeviceSocket.ReceiveTimeout = DEVICE_TIMEOUT;
            DeviceSocket.SendTimeout = DEVICE_TIMEOUT;
            DeviceStream.WriteTimeout = DEVICE_TIMEOUT;
            DeviceStream.ReadTimeout = DEVICE_TIMEOUT;
            EnableKeepAlive(socket.Client);
            // before the receiver, which disposes it when it ends
            PingWatchdog = new Timer(CheckPings, null, 1000, 1000);
            ReceiverThread = new Thread(ReceiverThreadFunc)
            {
                IsBackground = true
            };
            ReceiverThread.Start();
        }

        /// <summary>
        /// A connection whose device is gone, unplugged or switched off, gets no answer and no
        /// end: the system probes it after 10 seconds without traffic, every 5 seconds, and
        /// closes it after 3 probes without an answer
        /// </summary>
        private static void EnableKeepAlive(Socket socket)
        {
            try
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                // TcpKeepAliveTime, TcpKeepAliveRetryCount and TcpKeepAliveInterval: Windows 10
                // takes them as they are, .NET Core translates them for the other systems
                socket.SetSocketOption(SocketOptionLevel.Tcp, (SocketOptionName)3, 10);
                socket.SetSocketOption(SocketOptionLevel.Tcp, (SocketOptionName)16, 3);
                socket.SetSocketOption(SocketOptionLevel.Tcp, (SocketOptionName)17, 5);
            }
            catch (SocketException)
            {
                // an older Windows takes the times in milliseconds this way; the probes are 10
                try
                {
                    var values = new byte[12];
                    BitConverter.GetBytes(1).CopyTo(values, 0);
                    BitConverter.GetBytes(10000).CopyTo(values, 4);
                    BitConverter.GetBytes(5000).CopyTo(values, 8);
                    socket.IOControl(IOControlCode.KeepAliveValues, values, null);
                }
                catch (Exception)
                {
                    // the system's keepalive times, or none
                }
            }
        }

        private void CheckPings(object state)
        {
            if (Volatile.Read(ref Pinged) && !Volatile.Read(ref Closed)
                && DateTime.UtcNow - new DateTime(Interlocked.Read(ref LastReceived)) > PingSilence)
            {
                // the receiver ends and raises Disconnected
                Volatile.Write(ref Closed, true);
                DeviceSocket.Close();
            }
        }

        private async void ReceiverThreadFunc()
        {
            while (DeviceSocket.Connected && !CancellationTokenSource.IsCancellationRequested)
            {
                Response response = null;
                try
                {
                    response = await Response.FromStream(DeviceStream, CancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    DeviceError?.Invoke(this, ex);
                }
                if (response == null)
                {
                    break;
                }
                Interlocked.Exchange(ref LastReceived, DateTime.UtcNow.Ticks);
                var pending = Volatile.Read(ref Pending);
                if (response.ResponseCode == 0x7F)
                {
                    Volatile.Write(ref Pinged, true);
                    try
                    {
                        await Write(new PongCommand(response.DeviceID));
                    }
                    catch (Exception ex)
                    {
                        DeviceError?.Invoke(this, ex);
                    }
                    ReceivedPacket?.Invoke(this, response);
                }
                else if (pending != null && response.ResponseCode == pending.ResponseCode
                    && Interlocked.CompareExchange(ref Pending, null, pending) == pending)
                {
                    pending.Result.TrySetResult(response);
                }
                else
                {
                    ReceivedPacket?.Invoke(this, response);
                }
            }
            // closed by the device, by a timeout, by the silence after pings or by Dispose: in every
            // case nothing is read anymore
            PingWatchdog.Dispose();
            DeviceSocket.Close();
            Volatile.Write(ref Closed, true);
            Interlocked.Exchange(ref Pending, null)?.Result.TrySetResult(null);
            if (Interlocked.Exchange(ref DisconnectedRaised, 1) == 0)
            {
                Disconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        private async Task Write(Command cmd)
        {
            await WriteLock.WaitAsync();
            try
            {
                await cmd.Send(DeviceStream);
            }
            finally
            {
                WriteLock.Release();
            }
        }

        public async Task<Response> SendCommand(Command cmd)
        {
            await CommandLock.WaitAsync();
            PendingCommand pending = null;
            try
            {
                if (!DeviceSocket.Connected)
                {
                    throw new Exception("Device is not connected.");
                }
                // waiting before sending: the answer can arrive before Send returns
                pending = new PendingCommand(cmd.ResponseCode);
                Volatile.Write(ref Pending, pending);
                if (Volatile.Read(ref Closed))
                {
                    // the receiver stopped after the check above, it will not answer
                    throw new Exception("Device is not connected.");
                }
                await Write(cmd);
                using (var timeout = new CancellationTokenSource())
                {
                    if (await Task.WhenAny(pending.Result.Task, Task.Delay(DEVICE_TIMEOUT, timeout.Token)) != pending.Result.Task)
                    {
                        // a late answer would be taken for the one of the next command
                        if (DeviceSocket.Connected)
                        {
                            DeviceSocket.Close();
                        }
                        // Connected is false as soon as the command fails, not when the receiver notices
                        Volatile.Write(ref Closed, true);
                        throw new Exception("Device timeout waiting response.");
                    }
                    timeout.Cancel();
                }
                var result = await pending.Result.Task;
                if (result == null)
                {
                    throw new Exception("Device connection lost.");
                }
                if (result.Status != ResponseStatus.Success)
                {
                    throw new AnvizDeviceException(result.Status);
                }
                return result;
            }
            finally
            {
                if (pending != null)
                {
                    Interlocked.CompareExchange(ref Pending, null, pending);
                }
                CommandLock.Release();
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref Disposed, 1) == 1)
            {
                return;
            }
            CancellationTokenSource.Cancel();
            // a Disconnected handler can dispose the device from the receiver thread itself
            if (Thread.CurrentThread != ReceiverThread)
            {
                ReceiverThread.Join();
            }
            DeviceStream.Close();
            DeviceStream.Dispose();
            DeviceSocket.Close();
        }
    }
}
