using System.Linq;
using System.Text;

namespace Anviz.SDK.Utils
{
    static class Bytes
    {
        public static ulong Read(byte[] data)
        {
            ulong result = 0;
            for (int i = 0; i < data.Length; i++)
            {
                ulong b = (ulong)(data[data.Length - 1 - i] % 256);
                result += (ulong)(b << (byte)(i * 8));
            }
            return result;
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
