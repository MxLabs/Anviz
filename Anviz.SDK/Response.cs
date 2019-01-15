using System.Linq;

namespace Anviz.SDK
{
    public class Response
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
            STX = data.Take(1).First();
            CH = data.Skip(1).Take(4).ToArray();
            ACK = data.Skip(5).Take(1).First();
            RET = data.Skip(6).Take(1).First();
            LEN = data.Skip(7).Take(2).ToArray();
            DATA = data.Skip(9).Take(data.Length - 2).ToArray();
            CRC = data.Skip(data.Length - 2).Take(2).ToArray();
        }
    }
}
