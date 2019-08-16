using System.Net.Sockets;
using System.Threading.Tasks;

namespace Anviz.SDK
{
    public class AnvizManager
    {
        private readonly ulong DeviceId;

        public AnvizManager(ulong deviceId)
        {
            DeviceId = deviceId;
        }

        public async Task<AnvizDevice> Connect(string host, int port = 5010)
        {
            var DeviceSocket = new TcpClient();
            await DeviceSocket.ConnectAsync(host, port);
            return new AnvizDevice(DeviceSocket, DeviceId);
        }
    }
}
