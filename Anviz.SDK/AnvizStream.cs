using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Anviz.SDK
{
    internal class AnvizStream : IDisposable
    {
        private const int DEVICE_TIMEOUT = 10000; //10 seconds

        private readonly TcpClient DeviceSocket;
        private readonly NetworkStream DeviceStream;

        public AnvizStream(TcpClient socket)
        {
            DeviceSocket = socket;
            DeviceStream = socket.GetStream();
            DeviceSocket.ReceiveTimeout = DEVICE_TIMEOUT;
            DeviceSocket.SendTimeout = DEVICE_TIMEOUT;
            DeviceStream.WriteTimeout = DEVICE_TIMEOUT;
            DeviceStream.ReadTimeout = DEVICE_TIMEOUT;
        }

        public async Task<Response> SendCommand(Command cmd)
        {
            await cmd.Send(DeviceStream);
            return await Response.FromStream(cmd.ResponseCode, DeviceStream);
        }

        public void Dispose()
        {
            DeviceStream.Dispose();
            DeviceSocket.Close();
        }
    }
}
