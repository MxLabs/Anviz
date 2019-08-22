using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Anviz.SDK
{
    public class AnvizManager
    {
        private TcpListener server;

        public async Task<AnvizDevice> Connect(string host, int port = 5010)
        {
            var DeviceSocket = new TcpClient();
            await DeviceSocket.ConnectAsync(host, port);
            return await GetDevice(DeviceSocket);
        }

        public void Listen(int port = 5010)
        {
            Listen(new IPEndPoint(IPAddress.Any, port));
        }

        public void Listen(IPEndPoint localEP)
        {
            StopServer();
            server = new TcpListener(localEP);
            server.Start();
        }

        public void StopServer()
        {
            if (server == null) return;
            server.Stop();
            server = null;
        }

        public async Task<AnvizDevice> Accept()
        {
            var deviceSocket = await server.AcceptTcpClientAsync();
            return await GetDevice(deviceSocket);
        }

        private async Task<AnvizDevice> GetDevice(TcpClient DeviceSocket)
        {
            var device = new AnvizDevice(DeviceSocket);
            await device.GetDeviceID();
            return device;
        }
    }
}
