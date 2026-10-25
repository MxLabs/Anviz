using Anviz.SDK.Utils;
using System.Net;

namespace Anviz.SDK.Responses
{
    /// <summary>
    /// The server a device in client mode calls by name, and the DNS server it asks (0x0A, 0x0B)
    /// </summary>
    public class ServerUrl
    {
        public IPAddress Dns { get; set; }
        public string Url { get; set; }

        public ServerUrl()
        {
            Dns = IPAddress.Any;
            Url = "";
        }

        internal ServerUrl(byte[] data)
        {
            Dns = new IPAddress(Bytes.Split(data, 0, 4));
            Url = Bytes.GetAsciiString(Bytes.Split(data, 4, 100));
        }

        internal byte[] ToArray()
        {
            var ret = new byte[104];
            Dns.GetAddressBytes().CopyTo(ret, 0);
            Bytes.WriteAscii(100, Url).CopyTo(ret, 4);
            return ret;
        }
    }
}
