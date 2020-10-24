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

        private byte ResponseCode = 0;
        private TaskCompletionSource<Response> taskEmitter;

        public event EventHandler<Response> ReceivedPacket;
        public event EventHandler<Exception> DeviceError;

        public AnvizStream(TcpClient socket)
        {
            CancellationTokenSource = new CancellationTokenSource();
            taskEmitter = new TaskCompletionSource<Response>();
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
                    DeviceSocket.Close();
                    taskEmitter.TrySetResult(null);
                    break;
                }
                if (response.ResponseCode == 0x7F)
                {
                    await new PongCommand(response.DeviceID).Send(DeviceStream);
                    ReceivedPacket?.Invoke(this, response);
                }
                else if (response.ResponseCode == ResponseCode)
                {
                    ResponseCode = 0;
                    taskEmitter.TrySetResult(response);
                }
                else
                {
                    ReceivedPacket?.Invoke(this, response);
                }
            }
        }

        public async Task<Response> SendCommand(Command cmd)
        {
            if (!DeviceSocket.Connected)
            {
                throw new Exception("Device is not connected.");
            }
            ResponseCode = cmd.ResponseCode;
            await cmd.Send(DeviceStream);
            taskEmitter = new TaskCompletionSource<Response>();
            if (await Task.WhenAny(taskEmitter.Task, Task.Delay(DEVICE_TIMEOUT)) != taskEmitter.Task)
            {
                if (DeviceSocket.Connected)
                {
                    DeviceSocket.Close();
                }
                throw new Exception("Device timeout waiting response.");
            }
            var result = await taskEmitter.Task;
            if (result == null)
            {
                throw new Exception("Device connection lost.");
            }
            return result;
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
