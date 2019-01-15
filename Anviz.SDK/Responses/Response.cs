using Anviz.SDK.Utils;

namespace Anviz.SDK.Responses
{
    class Response
    {
        public byte STX { get; }
        public byte[] CH { get; }
        public byte ACK { get; }
        public byte RET { get; }
        public byte[] LEN { get; }
        public byte[] DATA { get; set; }
        public byte[] CRC { get; }

        public Response(byte[] data)
        {
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
