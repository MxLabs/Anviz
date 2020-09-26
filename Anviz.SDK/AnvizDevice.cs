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
        public event EventHandler<Record> ReceivedRecord;
        public event EventHandler<Exception> DeviceError;

        public AnvizDevice(TcpClient socket)
        {
            DeviceStream = new AnvizStream(socket);
            DeviceStream.ReceivedPacket += (s, e) =>
            {
                switch (e.ResponseCode)
                {
                    case 0x7F:
                        DevicePing?.Invoke(this, null);
                        break;
                    case 0xDF:
                        ReceivedRecord?.Invoke(this, new Record(e.DATA, 0));
                        break;
                    default:
                        ReceivedPacket?.Invoke(this, e);
                        break;
                }
            };
            DeviceStream.DeviceError += (s, e) =>
            {
                DeviceError?.Invoke(this, e);
            };
        }

        public void Dispose()
        {
            DeviceStream.Dispose();
        }
    }
}
