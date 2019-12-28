using Anviz.SDK.Responses;
using Anviz.SDK.Utils;
using System;
using System.Net.Sockets;

namespace Anviz.SDK
{
    public partial class AnvizDevice : IDisposable
    {
        public ulong DeviceId { get; private set; } = 0;
        public BiometricType DeviceBiometricType { get; private set; } = BiometricType.Unknown;
        private readonly AnvizStream DeviceStream;

        public event EventHandler DevicePing;
        public event EventHandler<Response> ReceivedPacket;

        public AnvizDevice(TcpClient socket)
        {
            DeviceStream = new AnvizStream(socket);
            DeviceStream.ReceivedPacket += (s, e) =>
            {
                if (e.ResponseCode == 0x7F)
                {
                    DevicePing?.Invoke(this, null);
                }
                else
                {
                    ReceivedPacket?.Invoke(this, e);
                }
            };
        }

        public void Dispose()
        {
            DeviceStream.Dispose();
        }
    }
}
