using Anviz.SDK.Utils;

namespace Anviz.SDK.Commands
{
    abstract class Command
    {
        public byte[] payload { get; private set; }

        protected ulong deviceId { get; }

        public Command(ulong deviceId)
        {
            this.deviceId = deviceId;
        }

        protected void BuildPayload(byte command, byte[] data)
        {
            ushort dataLength = (ushort)data.Length;
            ushort crc = 0x0000;
            byte[] commandBytes = new byte[8 + dataLength];
            commandBytes[0] = 0xA5;
            commandBytes[1] = (byte)((deviceId >> 24) % 256);
            commandBytes[2] = (byte)((deviceId >> 16) % 256);
            commandBytes[3] = (byte)((deviceId >> 8) % 256);
            commandBytes[4] = (byte)(deviceId % 256);
            commandBytes[5] = command;
            commandBytes[6] = (byte)(dataLength >> 8);
            commandBytes[7] = (byte)(dataLength % 256);
            data.CopyTo(commandBytes, 8);
            crc = CRC16.Compute(commandBytes);
            payload = new byte[commandBytes.Length + 2];
            commandBytes.CopyTo(payload, 0);
            payload[9 + dataLength] = (byte)((crc >> 8) % 256);
            payload[8 + dataLength] = (byte)(crc % 256);
        }
    }
}
