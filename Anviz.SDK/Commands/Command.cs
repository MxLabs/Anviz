using Anviz.SDK.Utils;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    abstract class Command
    {
        public byte ResponseCode { get; private set; }
        protected byte[] Payload { get; private set; }

        protected ulong DeviceId { get; }

        public Command(ulong deviceId)
        {
            DeviceId = deviceId;
        }

        protected void BuildPayload(byte command, byte[] data)
        {
            ResponseCode = (byte)(command + 0x80);
            var dataLength = (ushort)data.Length;
            if(dataLength > 400)
            {
                throw new Exception("Payload too big");
            }
            Payload = new byte[8 + dataLength + 2]; //preamble + data + crc
            Payload[0] = 0xA5;
            Payload[1] = (byte)((DeviceId >> 24) % 256);
            Payload[2] = (byte)((DeviceId >> 16) % 256);
            Payload[3] = (byte)((DeviceId >> 8) % 256);
            Payload[4] = (byte)(DeviceId % 256);
            Payload[5] = command;
            Payload[6] = (byte)(dataLength >> 8);
            Payload[7] = (byte)(dataLength % 256);
            data.CopyTo(Payload, 8);
            var crc = CRC16.Compute(Payload, Payload.Length - 2); //last 2 bytes = crc
            Payload[8 + dataLength] = (byte)(crc % 256);
            Payload[9 + dataLength] = (byte)((crc >> 8) % 256);
        }

        public async Task Send(NetworkStream stream)
        {
            await stream.WriteAsync(Payload, 0, Payload.Length);
        }
    }
}
