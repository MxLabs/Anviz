using Anviz.SDK.Utils;
using System.Net;
using System.Net.NetworkInformation;

namespace Anviz.SDK.Responses
{
    /// <summary>
    /// A device that answered a UDP search (0x02): what it says of its network, and the address
    /// it answered from
    /// </summary>
    public class DiscoveredDevice
    {
        public IPAddress AnsweredFrom { get; }
        public ulong DeviceId { get; }
        public string Model { get; }
        public string SerialNumber { get; }
        public IPAddress IP { get; }
        public IPAddress SubnetMask { get; }
        public IPAddress DefaultGateway { get; }
        public PhysicalAddress MacAddress { get; }
        public IPAddress Server { get; }
        public ulong Port { get; }
        public TcpMode Mode { get; }
        public string Firmware { get; }
        /// <summary>
        /// The DNS server and the server's URL, of a firmware that calls a server by name; null
        /// for one that does not
        /// </summary>
        public IPAddress Dns { get; }
        public string Url { get; }

        internal DiscoveredDevice(IPAddress answeredFrom, ulong deviceId, byte[] data)
        {
            AnsweredFrom = answeredFrom;
            DeviceId = deviceId;
            Model = Bytes.GetAsciiString(Bytes.Split(data, 0, 10));
            SerialNumber = Bytes.GetAsciiString(Bytes.Split(data, 10, 16));
            IP = new IPAddress(Bytes.Split(data, 26, 4));
            SubnetMask = new IPAddress(Bytes.Split(data, 30, 4));
            DefaultGateway = new IPAddress(Bytes.Split(data, 34, 4));
            MacAddress = new PhysicalAddress(Bytes.Split(data, 38, 6));
            Server = new IPAddress(Bytes.Split(data, 44, 4));
            Port = Bytes.Read(Bytes.Split(data, 48, 2));
            Mode = (TcpMode)data[50];
            Firmware = Bytes.GetAsciiString(Bytes.Split(data, 51, 8));
            if (data.Length >= 167)
            {
                Dns = new IPAddress(Bytes.Split(data, 63, 4));
                Url = Bytes.GetAsciiString(Bytes.Split(data, 67, 100));
            }
        }
    }
}
