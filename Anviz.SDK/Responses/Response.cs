using Anviz.SDK.Utils;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Anviz.SDK.Responses
{
    class Response
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

        internal Response(byte responseCode, byte[] data)
        {
            var STX = data[0];
            if (STX != 0xA5)
            {
                throw new Exception("Invalid header");
            }
            var CH = Bytes.Split(data, 1, 4);
            DeviceID = Bytes.Read(CH);
            var ACK = data[5];
            if (ACK != responseCode)
            {
                throw new Exception("Invalid ACK");
            }
            var RET = (RetVal)data[6];
            if (RET != RetVal.SUCCESS)
            {
                throw new Exception("RET: " + RET.ToString());
            }
            var LEN = (int)Bytes.Read(Bytes.Split(data, 7, 2));
            DATA = Bytes.Split(data, 9, LEN);
            var CRC = Bytes.Split(data, LEN + 9, 2);
            var ComputedCRC = (CRC[1] << 8) + CRC[0];
            var ExpectedCRC = CRC16.Compute(data, LEN + 9);
            if (ComputedCRC != ExpectedCRC)
            {
                throw new Exception("Invalid CRC");
            }
        }

        internal static async Task<Response> FromStream(byte ResponseCode, NetworkStream stream)
        {
            /*
             * Ethernet MTU is slightly less than 1500
             * Since payload maximum size is around 600 bytes,
             * we can safely assume that we get all data with 1 read.
             * A better way is done by reading the first 10 bytes,
             * building the response vars and reading the LEN value
             */
            var data = new byte[1500];
            await stream.ReadAsync(data, 0, data.Length);
            return new Response(ResponseCode, data);
        }
    }
}
