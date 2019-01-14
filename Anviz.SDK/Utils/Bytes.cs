using System.Linq;

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
    }
}
