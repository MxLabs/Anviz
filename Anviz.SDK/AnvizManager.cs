using System.Net.Sockets;
using System.Threading.Tasks;

namespace Anviz.SDK
{
    public class AnvizManager
    {
        public async Task<AnvizDevice> Connect(string host, int port = 5010)
        {
            var DeviceSocket = new TcpClient();
            await DeviceSocket.ConnectAsync(host, port);
            var device = new AnvizDevice(DeviceSocket);
            await device.GetDeviceID();
            return device;
        }
    }
}
