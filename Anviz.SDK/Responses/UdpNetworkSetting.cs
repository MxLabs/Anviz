using Anviz.SDK.Utils;
using System.Net;
using System.Net.NetworkInformation;

namespace Anviz.SDK.Responses
{
    /// <summary>
    /// The network a device is given over UDP (0x03), by its MAC address: a device takes it only
    /// when the MAC is its own and the user and password are its own
    /// </summary>
    public class UdpNetworkSetting
    {
        public PhysicalAddress MacAddress { get; set; }
        public IPAddress IP { get; set; }
        public IPAddress SubnetMask { get; set; }
        public IPAddress DefaultGateway { get; set; }
        public IPAddress Server { get; set; }
        public ulong Port { get; set; } = 5010;
        public TcpMode Mode { get; set; }
        public ulong DeviceId { get; set; }
        public string User { get; set; } = "";
        public string Password { get; set; } = "";
        /// <summary>
        /// The DNS server and the server's URL, for a firmware that calls a server by name; null
        /// leaves them out
        /// </summary>
        public IPAddress Dns { get; set; }
        public string Url { get; set; }

        /// <summary>
        /// The setting of a device as a search found it, to change what is wanted
        /// </summary>
        public static UdpNetworkSetting From(DiscoveredDevice device)
        {
            return new UdpNetworkSetting
            {
                MacAddress = device.MacAddress,
                IP = device.IP,
                SubnetMask = device.SubnetMask,
                DefaultGateway = device.DefaultGateway,
                Server = device.Server,
                Port = device.Port,
                Mode = device.Mode,
                DeviceId = device.DeviceId,
                Dns = device.Dns,
                Url = device.Url
            };
        }

        internal byte[] ToArray()
        {
            var ret = new byte[Url != null ? 161 : 57];
            IP.GetAddressBytes().CopyTo(ret, 0);
            SubnetMask.GetAddressBytes().CopyTo(ret, 4);
            DefaultGateway.GetAddressBytes().CopyTo(ret, 8);
            MacAddress.GetAddressBytes().CopyTo(ret, 12);
            Server.GetAddressBytes().CopyTo(ret, 18);
            Bytes.Write(2, Port).CopyTo(ret, 22);
            ret[24] = (byte)Mode;
            Bytes.Write(4, DeviceId).CopyTo(ret, 25);
            Bytes.WriteAscii(12, User).CopyTo(ret, 33);
            Bytes.WriteAscii(12, Password).CopyTo(ret, 45);
            if (Url != null)
            {
                (Dns ?? IPAddress.Any).GetAddressBytes().CopyTo(ret, 57);
                Bytes.WriteAscii(100, Url).CopyTo(ret, 61);
            }
            return ret;
        }
    }
}
