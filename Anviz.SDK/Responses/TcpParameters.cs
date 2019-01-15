using Anviz.SDK.Utils;
using System.Net.Sockets;

namespace Anviz.SDK.Responses
{
    public class TcpParameters : Response
    {
        public string IP { get; }
        public string SubnetMask { get; }
        public string MacAddress { get; }
        public string DefaultGateway { get; }
        public string Server { get; }
        public int FarLimit { get; }
        public int ComPort { get; }
        public string TcpMode { get; }
        public int DhcpLimit { get; }

        public TcpParameters(NetworkStream stream) : base(stream)
        {
            IP = string.Join(".", Bytes.Split(DATA, 0, 4));
            SubnetMask = string.Join(".", Bytes.Split(DATA, 4, 4));
            MacAddress = string.Join("-", Bytes.Split(DATA, 8, 6));
            DefaultGateway = string.Join(".", Bytes.Split(DATA, 14, 4));
            Server = string.Join(".", Bytes.Split(DATA, 18, 4));
            FarLimit = DATA[23];
            ComPort = (int)Bytes.Read(Bytes.Split(DATA, 23, 2));
            TcpMode = DATA[25] == 0 ? "Server Mode" : "Client Mode";
            DhcpLimit = DATA[26];
        }
    }
}
