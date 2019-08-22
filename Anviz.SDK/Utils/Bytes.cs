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
            var ret = new byte[length];
            while (value > 0)
            {
                ret[--length] = (byte)(value % 256);
                value >>= 8;
            }
            return ret;
        }

        public static ulong PasswordRead(byte[] pwd)
        {
            var ret = (ulong)(pwd[0] & 0x0F); //first 4 bits are pwdlen
            ret = (ret << 8) | pwd[1];
            ret = (ret << 8) | pwd[2];
            return ret;
        }

        public static byte[] PasswordWrite(ulong? pwd)
        {
            if (!pwd.HasValue)
            {
                return new byte[] { 0xFF, 0xFF, 0xFF };
            }
            var pwdlen = (byte)pwd.ToString().Length;
            var ret = new byte[3];
            ret[2] = (byte)(pwd % 256);
            pwd >>= 8;
            ret[1] = (byte)(pwd % 256);
            pwd >>= 8;
            ret[0] = (byte)(pwdlen << 4);
            ret[0] |= (byte)((pwd % 256) & 0x0F);
            return ret;
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
