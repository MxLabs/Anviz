using Anviz.SDK.Utils;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Anviz.SDK.Responses
{
    public class Response
    {
        enum RetVal
        {
            SUCCESS = 0x00, // operation successful
            FAIL = 0x01, // operation failed
            FULL = 0x04, // user full
            EMPTY = 0x05, // user empty
            NO_USER = 0x06, // user not exist
            TIME_OUT = 0x08, // capture timeout
            USER_OCCUPIED = 0x0A, // user already exists
            FINGER_OCCUPIED = 0x0B, // fingerprint already exists
        }

        public ulong DeviceID { get; }

        public byte[] DATA { get; }

        public byte ResponseCode { get; }

        internal Response(byte[] data, ulong deviceId, byte ACK)
        {
            DATA = data;
            DeviceID = deviceId;
            ResponseCode = ACK;
        }

        internal static async Task<Response> FromStream(NetworkStream stream, CancellationToken ct)
        {
            var base_offset = 6;
            var data = new byte[16000]; //for facepass7 template size is 15360
            try
            {
                var amount = await stream.ReadAsync(data, 0, base_offset, ct);
                if (amount == 0)
                {
                    return null;
                }
                if (amount != base_offset)
                {
                    throw new Exception("Partial packet read");
                }
            }
            catch (Exception)
            {
                return null;
            }

            var STX = data[0];
            if (STX != 0xA5)
            {
                throw new Exception("Invalid header");
            }
            var CH = Bytes.Split(data, 1, 4);
            var ACK = data[5];
            if (ACK < 0x80)
            {
                if (await stream.ReadAsync(data, base_offset, 2, ct) != 2)
                {
                    throw new Exception("Partial packet read");
                }
                base_offset += 2;
            }
            else
            {
                if (await stream.ReadAsync(data, base_offset, 3, ct) != 3)
                {
                    throw new Exception("Partial packet read");
                }
                var RET = (RetVal)data[6];
                if (RET != RetVal.SUCCESS)
                {
                    throw new Exception("RET: " + RET.ToString());
                }
                base_offset += 3;
            }
            var LEN = (int)Bytes.Read(Bytes.Split(data, 7, 2));
            var P_LEN = LEN + 2;

            await ReadAsyncContinuously(stream, data, base_offset, P_LEN, ct);

            var PacketData = Bytes.Split(data, base_offset, LEN);
            var CRC = Bytes.Split(data, LEN + base_offset, 2);
            var ComputedCRC = (CRC[1] << 8) + CRC[0];
            var ExpectedCRC = CRC16.Compute(data, LEN + base_offset);
            if (ComputedCRC != ExpectedCRC)
            {
                throw new Exception("Invalid CRC");
            }
            return new Response(PacketData, Bytes.Read(CH), ACK);
        }

        private static async Task ReadAsyncContinuously(NetworkStream stream, byte[] data, int offset, int size, CancellationToken ct)
        {
            int readed = 0;
            do
            {
                var amount = await stream.ReadAsync(data, offset + readed, size - readed, ct);
                if (amount == 0)
                {
                    throw new Exception("Partial packet read");
                }
                readed += amount;
            } while (readed < size);
        }
    }
}
