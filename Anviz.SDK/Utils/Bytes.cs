using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anviz.SDK.Utils
{
    static class Bytes
    {
        public static ulong Read(byte[] data)
        {
            ulong result = 0;
            for (var i = 0; i < data.Length; i++)
            {
                var b = (ulong)(data[data.Length - 1 - i] % 256);
                result += b << (byte)(i * 8);
            }
            return result;
        }

        public static byte[] Write(int length, ulong value)
        {
            var ret = new List<byte>();
            while (value > 0)
            {
                ret.Add((byte)(value % 256));
                value >>= 8;
            }
            while (length != ret.Count)
            {
                ret.Add(0);
            }
            ret.Reverse();
            return ret.ToArray();
        }

        public static byte[] Split(byte[] data, int start, int count)
        {
            return data.Skip(start).Take(count).ToArray();
        }

        public static string GetUnicodeString(byte[] data)
        {
            return Encoding.BigEndianUnicode.GetString(data).TrimEnd('\0');
        }

        public static string GetAsciiString(byte[] data)
        {
            return Encoding.ASCII.GetString(data).TrimEnd('\0');
        }
    }
}
