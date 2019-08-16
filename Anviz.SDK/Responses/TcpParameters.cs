using Anviz.SDK.Utils;
using System;

namespace Anviz.SDK.Responses
{
    public enum TcpMode
    {
        Server = 0,
        Client = 1
    }

    public class TcpParameters
    {
        public string IP { get; }
        public string SubnetMask { get; }
        public string MacAddress { get; }
        public string DefaultGateway { get; }
        public string Server { get; }
        public byte FarLimit { get; }
        public ulong ComPort { get; }
        public TcpMode TcpMode { get; }
        public bool DhcpEnabled { get; }

        public TcpParameters(byte[] data)
        {
            IP = string.Join(".", Bytes.Split(data, 0, 4));
            SubnetMask = string.Join(".", Bytes.Split(data, 4, 4));
            MacAddress = BitConverter.ToString(Bytes.Split(data, 8, 6));
            DefaultGateway = string.Join(".", Bytes.Split(data, 14, 4));
            Server = string.Join(".", Bytes.Split(data, 18, 4));
            FarLimit = data[22];
            ComPort = Bytes.Read(Bytes.Split(data, 23, 2));
            TcpMode = (TcpMode)data[25];
            DhcpEnabled = data[26] > 0;
        }
    }
}
