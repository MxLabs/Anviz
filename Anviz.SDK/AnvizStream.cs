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
            while (!CancellationTokenSource.IsCancellationRequested)
            {
                var response = await Response.FromStream(DeviceStream, CancellationTokenSource.Token);
                if (response.ResponseCode == 0x7F)
                {
                    Console.WriteLine("Received ping");
                }
                else if (response.ResponseCode == ResponseCode)
                {
                    ResponseCode = 0;
                    if (taskEmitter != null)
                    {
                        taskEmitter.TrySetResult(response);
                    }
                }
                else
                {
                    Console.WriteLine("Received unsolecited packet");
                }
            }
        }

        public async Task<Response> SendCommand(Command cmd)
        {
            ResponseCode = cmd.ResponseCode;
            await cmd.Send(DeviceStream);
            taskEmitter = new TaskCompletionSource<Response>();
            return await taskEmitter.Task;
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
