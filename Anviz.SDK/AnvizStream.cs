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
        private int DisconnectedRaised;

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
            ReceiverThread = new Thread(ReceiverThreadFunc)
            {
                IsBackground = true
            };
            ReceiverThread.Start();
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
                var pending = Volatile.Read(ref Pending);
                if (response.ResponseCode == 0x7F)
                {
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
            // closed by the device, by a timeout or by Dispose: in every case nothing is read anymore
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
            CancellationTokenSource.Cancel();
            ReceiverThread.Join();
            DeviceStream.Close();
            DeviceStream.Dispose();
            DeviceSocket.Close();
            CancellationTokenSource.Dispose();
        }
    }
}
