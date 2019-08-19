using Anviz.SDK.Utils;
using System.Net;
using System.Net.NetworkInformation;

namespace Anviz.SDK.Responses
{
    public enum TcpMode
    {
        Server = 0,
        Client = 1
    }

    public class TcpParameters
    {
        public IPAddress IP { get; set; }
        public IPAddress SubnetMask { get; set; }
        public PhysicalAddress MacAddress { get; set; }
        public IPAddress DefaultGateway { get; set; }
        public IPAddress Server { get; set; }
        public byte FarLimit { get; set; }
        public ulong ComPort { get; set; }
        public TcpMode TcpMode { get; set; }
        public bool DhcpEnabled { get; set; }

        internal TcpParameters(byte[] data)
        {
            IP = new IPAddress(Bytes.Split(data, 0, 4));
            SubnetMask = new IPAddress(Bytes.Split(data, 4, 4));
            MacAddress = new PhysicalAddress(Bytes.Split(data, 8, 6));
            DefaultGateway = new IPAddress(Bytes.Split(data, 14, 4));
            Server = new IPAddress(Bytes.Split(data, 18, 4));
            FarLimit = data[22];
            ComPort = Bytes.Read(Bytes.Split(data, 23, 2));
            TcpMode = (TcpMode)data[25];
            DhcpEnabled = data[26] > 0;
        }

        internal byte[] ToArray()
        {
            var ret = new byte[27];
            IP.GetAddressBytes().CopyTo(ret, 0);
            SubnetMask.GetAddressBytes().CopyTo(ret, 4);
            MacAddress.GetAddressBytes().CopyTo(ret, 8);
            DefaultGateway.GetAddressBytes().CopyTo(ret, 14);
            Server.GetAddressBytes().CopyTo(ret, 18);
            ret[22] = FarLimit;
            Bytes.Write(2, ComPort).CopyTo(ret, 23);
            ret[25] = (byte)TcpMode;
            ret[26] = (byte)(DhcpEnabled ? 1 : 0);
            return ret;
        }
    }
}
