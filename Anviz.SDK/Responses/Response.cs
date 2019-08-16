using Anviz.SDK.Utils;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Anviz.SDK.Responses
{
    class Response
    {
        private const byte RET_SUCCESS = 0x00;
        private const byte RET_FAIL = 0x01;

        public ulong DeviceID { get; }

        public byte[] DATA { get; }

        public Response(byte responseCode, byte[] data)
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
            var RET = data[6];
            if (RET != RET_SUCCESS)
            {
                throw new Exception("Invalid RET");
            }
            var LEN = (int)Bytes.Read(Bytes.Split(data, 7, 2));
            DATA = Bytes.Split(data, 9, LEN);
            var CRC = Bytes.Split(data, LEN + 9, 2);
            var ComputedCRC = (CRC[1] << 8) + CRC[0];
            var ExpectedCRC = CRC16.Compute(data, LEN + 9);
            if(ComputedCRC != ExpectedCRC)
            {
                throw new Exception("Invalid CRC");
            }
        }

        public static async Task<Response> FromStream(byte ResponseCode, NetworkStream stream)
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
