using Anviz.SDK.Utils;
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
            var payloadLength = 8 + dataLength + 2; //preamble + data + crc
            if (command > 0x80)
            {
                /* this case is actually a response, but since it is 
                 * used only for Pong, we threat it as a special case
                 * instead of creating a full ResponseBuildPayload
                 */
                payloadLength += 1; // RET
            }
            /* NOTE: in the original specification, the payload must be less than 400 bytes
             * However, issue #45 found that Facepass devices use much bigger payloads
             * 
             * if (dataLength > 400)
             * {
             *     throw new Exception("Payload too big");
             * }
             */
            var i = 0;
            Payload = new byte[payloadLength];
            Payload[i++] = 0xA5;
            Payload[i++] = (byte)((DeviceId >> 24) % 256);
            Payload[i++] = (byte)((DeviceId >> 16) % 256);
            Payload[i++] = (byte)((DeviceId >> 8) % 256);
            Payload[i++] = (byte)(DeviceId % 256);
            Payload[i++] = command;
            if (command > 0x80)
            {
                Payload[i++] = 0; // RET
            }
            Payload[i++] = (byte)(dataLength >> 8);
            Payload[i++] = (byte)(dataLength % 256);
            data.CopyTo(Payload, i);
            i += data.Length;
            var crc = CRC16.Compute(Payload, Payload.Length - 2); //last 2 bytes = crc
            Payload[i++] = (byte)(crc % 256);
            Payload[i++] = (byte)((crc >> 8) % 256);
        }

        public async Task Send(NetworkStream stream)
        {
            await stream.WriteAsync(Payload, 0, Payload.Length);
        }
    }
}
