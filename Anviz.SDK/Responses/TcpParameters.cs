using Anviz.SDK.Utils;

namespace Anviz.SDK.Responses
{
    public class TcpParameters
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

        public TcpParameters(byte[] data)
        {
            IP = string.Join(".", Bytes.Split(data, 0, 4));
            SubnetMask = string.Join(".", Bytes.Split(data, 4, 4));
            MacAddress = string.Join("-", Bytes.Split(data, 8, 6));
            DefaultGateway = string.Join(".", Bytes.Split(data, 14, 4));
            Server = string.Join(".", Bytes.Split(data, 18, 4));
            FarLimit = data[23];
            ComPort = (int)Bytes.Read(Bytes.Split(data, 23, 2));
            TcpMode = data[25] == 0 ? "Server Mode" : "Client Mode";
            DhcpLimit = data[26];
        }
    }
}
