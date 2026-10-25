using Anviz.SDK.Utils;
using System;

namespace Anviz.SDK.Responses
{
    /// <summary>
    /// One of the five WiFi networks a device knows (0x6E, 0x6F): its SSID and password
    /// </summary>
    public class WifiNetwork
    {
        public const int MAX_NETWORKS = 5;

        public byte Number { get; }
        public string Ssid { get; set; }
        public string Password { get; set; }

        public WifiNetwork(byte number)
        {
            if (number < 1 || number > MAX_NETWORKS)
            {
                throw new ArgumentOutOfRangeException("number", "Must be within 1 to WifiNetwork.MAX_NETWORKS");
            }
            Number = number;
            Ssid = "";
            Password = "";
        }

        internal WifiNetwork(byte number, byte[] data)
        {
            Number = number;
            Ssid = Bytes.GetAsciiString(Bytes.Split(data, 0, 64));
            Password = Bytes.GetAsciiString(Bytes.Split(data, 64, 64));
        }

        internal byte[] ToArray()
        {
            var ret = new byte[129];
            ret[0] = Number;
            Bytes.WriteAscii(64, Ssid).CopyTo(ret, 1);
            Bytes.WriteAscii(64, Password).CopyTo(ret, 65);
            return ret;
        }
    }
}
