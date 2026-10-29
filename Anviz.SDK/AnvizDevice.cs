using Anviz.SDK.Responses;
using Anviz.SDK.Utils;
using System;
using System.Linq;
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

        /// <summary>
        /// Raised once when the connection ends: closed by the device, lost, after a timeout or by
        /// Dispose. Check <see cref="Connected"/> after subscribing, the connection may be already over
        /// </summary>
        public event EventHandler Disconnected;

        public bool Connected => DeviceStream.Connected;

        internal TimeSpan PingSilence
        {
            set => DeviceStream.PingSilence = value;
        }

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
                        if (!IsRepeatedRecord(e.DATA))
                        {
                            ReceivedRecord?.Invoke(this, new Record(e.DATA, 0));
                        }
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
            DeviceStream.Disconnected += (s, e) =>
            {
                Disconnected?.Invoke(this, EventArgs.Empty);
            };
        }

        /// <summary>
        /// A W2PRO sends every record in real time twice, 400 to 600 ms apart, also to CrossChex,
        /// which never answers it. A record has the second of the verification, so the same bytes
        /// within the window are the same record. Called by the receiver thread only
        /// </summary>
        private bool IsRepeatedRecord(byte[] data)
        {
            var now = DateTime.UtcNow;
            var repeated = lastRecord != null && now - lastRecordAt < REPEATED_RECORD_WINDOW
                && lastRecord.SequenceEqual(data);
            lastRecord = data;
            lastRecordAt = now;
            return repeated;
        }

        private static readonly TimeSpan REPEATED_RECORD_WINDOW = TimeSpan.FromSeconds(2);
        private byte[] lastRecord;
        private DateTime lastRecordAt;

        public void Dispose()
        {
            DeviceStream.Dispose();
        }
    }
}
