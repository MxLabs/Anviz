using Anviz.SDK.Utils;

namespace Anviz.SDK.Responses
{
    /// <summary>
    /// The vendor's name, address and tax number a device keeps (0x7A, 0x7B)
    /// </summary>
    public class ExtendedInfo
    {
        public string VendorName { get; set; }
        public string VendorAddress { get; set; }
        public string TaxNumber { get; set; }
        private readonly byte[] reserved;

        internal ExtendedInfo(byte[] data)
        {
            VendorName = Bytes.GetUnicodeString(Bytes.Split(data, 0, 50));
            VendorAddress = Bytes.GetUnicodeString(Bytes.Split(data, 50, 100));
            TaxNumber = Bytes.GetAsciiString(Bytes.Split(data, 150, 15));
            reserved = Bytes.Split(data, 165, 155);
        }

        internal byte[] ToArray()
        {
            var ret = new byte[320];
            Bytes.WriteUnicode(50, VendorName).CopyTo(ret, 0);
            Bytes.WriteUnicode(100, VendorAddress).CopyTo(ret, 50);
            Bytes.WriteAscii(15, TaxNumber).CopyTo(ret, 150);
            reserved.CopyTo(ret, 165);
            return ret;
        }
    }
}
