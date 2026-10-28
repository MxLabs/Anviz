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
            if (length < 8 && value >> (length * 8) != 0)
            {
                throw new System.ArgumentOutOfRangeException("value", value, $"Does not fit in {length} bytes");
            }
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

        /// <summary>
        /// Reads a big endian UTF-16 string that ends at its first zero character: devices leave
        /// bytes of an older value after it
        /// </summary>
        public static string GetUnicodeString(byte[] data)
        {
            var text = Encoding.BigEndianUnicode.GetString(data);
            var end = text.IndexOf('\0');
            return end < 0 ? text : text.Substring(0, end);
        }

        /// <summary>
        /// Reads an ASCII string that ends at its first zero byte
        /// </summary>
        public static string GetAsciiString(byte[] data)
        {
            var text = Encoding.ASCII.GetString(data);
            var end = text.IndexOf('\0');
            return end < 0 ? text : text.Substring(0, end);
        }

        /// <summary>
        /// Writes a string as big endian UTF-16 in length bytes, padded with zeros
        /// </summary>
        public static byte[] WriteUnicode(int length, string value)
        {
            var ret = new byte[length];
            var bytes = Encoding.BigEndianUnicode.GetBytes(value ?? "");
            System.Array.Copy(bytes, ret, System.Math.Min(bytes.Length, length));
            return ret;
        }

        /// <summary>
        /// Writes a string as ASCII in length bytes, padded with zeros
        /// </summary>
        public static byte[] WriteAscii(int length, string value)
        {
            var ret = new byte[length];
            var bytes = Encoding.ASCII.GetBytes(value ?? "");
            System.Array.Copy(bytes, ret, System.Math.Min(bytes.Length, length));
            return ret;
        }

        /// <summary>
        /// The encoding of the fields the protocol calls ANSI. It names no code page; Latin-1 keeps
        /// the accents of western languages, which ASCII turns into '?'
        /// </summary>
        public static readonly Encoding Ansi = Encoding.GetEncoding(28591);

        /// <summary>
        /// Reads an ANSI string that ends at its first zero byte
        /// </summary>
        public static string GetAnsiString(byte[] data)
        {
            var text = Ansi.GetString(data);
            var end = text.IndexOf('\0');
            return end < 0 ? text : text.Substring(0, end);
        }

        /// <summary>
        /// Writes a string as ANSI in length bytes, padded with zeros
        /// </summary>
        public static byte[] WriteAnsi(int length, string value)
        {
            var ret = new byte[length];
            var bytes = Ansi.GetBytes(value ?? "");
            System.Array.Copy(bytes, ret, System.Math.Min(bytes.Length, length));
            return ret;
        }

        /// <summary>
        /// The four bytes of an IPv4 address: the protocol has no room for another kind
        /// </summary>
        public static byte[] IPv4(System.Net.IPAddress address)
        {
            if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            {
                throw new System.ArgumentException("The device takes IPv4 addresses only: " + address);
            }
            return address.GetAddressBytes();
        }

        /// <summary>
        /// Reads a BCD byte: 0x22 is 22
        /// </summary>
        public static int ReadBcd(byte value)
        {
            return (value >> 4) * 10 + (value & 0x0F);
        }

        /// <summary>
        /// Writes a number from 0 to 99 as a BCD byte: 22 is 0x22
        /// </summary>
        public static byte WriteBcd(int value)
        {
            return (byte)((value / 10) << 4 | (value % 10));
        }
    }
}
