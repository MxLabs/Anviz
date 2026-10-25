using Anviz.SDK.Utils;
using System;

namespace Anviz.SDK.Responses
{
    /// <summary>
    /// A message the device shows to a user between two dates (0x56, 0x57), one of 50: in ANSI,
    /// 48 bytes of text, or in UTF-16, 96 bytes, as the device keeps them
    /// </summary>
    public class DeviceMessage
    {
        public const int MAX_MESSAGES = 50;

        public ulong UserId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string Text { get; set; }
        public bool Unicode { get; set; }

        public DeviceMessage()
        {
            Text = "";
        }

        internal DeviceMessage(byte[] data)
        {
            Unicode = data.Length > 59;
            UserId = Bytes.Read(Bytes.Split(data, 0, 5));
            From = ReadDate(data, 5);
            To = ReadDate(data, 8);
            var text = Bytes.Split(data, 11, data.Length - 11);
            Text = Unicode ? Bytes.GetUnicodeString(text) : Bytes.GetAsciiString(text);
        }

        internal static DateTime ReadDate(byte[] data, int offset)
        {
            return new DateTime(2000 + data[offset], data[offset + 1], data[offset + 2]);
        }

        internal static void WriteDate(DateTime date, byte[] ret, int offset)
        {
            ret[offset] = (byte)(date.Year - 2000);
            ret[offset + 1] = (byte)date.Month;
            ret[offset + 2] = (byte)date.Day;
        }

        internal byte[] ToArray()
        {
            var ret = new byte[Unicode ? 107 : 59];
            Bytes.Write(5, UserId).CopyTo(ret, 0);
            WriteDate(From, ret, 5);
            WriteDate(To, ret, 8);
            var size = ret.Length - 11;
            var text = Unicode ? Bytes.WriteUnicode(size, Text) : Bytes.WriteAscii(size, Text);
            text.CopyTo(ret, 11);
            return ret;
        }
    }

    /// <summary>
    /// Whose a message is and when the device shows it, without its text (0x58)
    /// </summary>
    public class DeviceMessageHeader
    {
        public byte Index { get; }
        public ulong UserId { get; }
        public DateTime From { get; }
        public DateTime To { get; }

        internal DeviceMessageHeader(byte index, byte[] data)
        {
            Index = index;
            UserId = Bytes.Read(Bytes.Split(data, 0, 5));
            From = DeviceMessage.ReadDate(data, 5);
            To = DeviceMessage.ReadDate(data, 8);
        }
    }
}
