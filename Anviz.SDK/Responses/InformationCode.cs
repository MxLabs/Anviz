using Anviz.SDK.Utils;

namespace Anviz.SDK.Responses
{
    /// <summary>
    /// The text a device shows as its maker or owner (0x4A, 0x4B): 10 ANSI bytes, or 20 bytes of
    /// UTF-16, as the device keeps it
    /// </summary>
    public class InformationCode
    {
        public string Text { get; set; }
        public bool Unicode { get; }

        public InformationCode(string text, bool unicode)
        {
            Text = text;
            Unicode = unicode;
        }

        internal InformationCode(byte[] data)
        {
            Unicode = data.Length > 10;
            Text = Unicode ? Bytes.GetUnicodeString(data) : Bytes.GetAsciiString(data);
        }

        internal byte[] ToArray()
        {
            return Unicode ? Bytes.WriteUnicode(20, Text) : Bytes.WriteAscii(10, Text);
        }
    }
}
