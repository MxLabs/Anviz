namespace Anviz.SDK
{
    public class Response
    {
        public byte STX { get; set; }
        public byte[] CH { get; set; }
        public byte ACK { get; set; }
        public byte RET { get; set; }
        public byte[] LEN { get; set; }
        public byte[] DATA { get; set; }
        public byte[] CRC { get; set; }

    }
}
