using Anviz.SDK.Utils;
using System.Net.Sockets;

namespace Anviz.SDK.Responses
{
    class Response
    {
        private const byte RET_SUCCESS = 0x00;
        private const byte RET_FAIL = 0x01;

        public byte STX { get; }
        public byte[] CH { get; }
        public byte ACK { get; }
        public byte RET { get; }
        public byte[] LEN { get; }
        public byte[] DATA { get; set; }
        public byte[] CRC { get; }

        public bool IsValid {
            get => RET == RET_SUCCESS;
        }

        public Response(NetworkStream stream)
        {
            /*
             * Ethernet MTU is slightly less than 1500
             * Since payload maximum size is around 600 bytes,
             * we can safely assume that we get all data with 1 read.
             * A better way is done by reading the first 10 bytes,
             * building the response vars and reading the LEN value
             */
            byte[] data = new byte[1500];
            stream.Read(data, 0, data.Length);
            
            STX = data[0];
            CH = Bytes.Split(data, 1, 4);
            ACK = data[5];
            RET = data[6];
            LEN = Bytes.Split(data, 7, 2);
            DATA = Bytes.Split(data, 9, data.Length - 2);
            CRC = Bytes.Split(data, data.Length - 2, 2);
        }
    }
}
