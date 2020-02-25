using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Anviz.SDK
{
    public class AnvizManager
    {
        private TcpListener server;

        public string ConnectionUser { get; set; } = "admin";
        public string ConnectionPassword { get; set; } = "12345";
        public bool AuthenticateConnection { get; set; } = false;

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

        public bool Pending()
        {
            return server.Pending();
        }

        public async Task<AnvizDevice> Accept()
        {
            var deviceSocket = await server.AcceptTcpClientAsync();
            return await GetDevice(deviceSocket);
        }

        private async Task<AnvizDevice> GetDevice(TcpClient DeviceSocket)
        {
            var device = new AnvizDevice(DeviceSocket);
            if (AuthenticateConnection)
            {
                await device.SetConnectionPassword(ConnectionUser, ConnectionPassword);
            }
            await device.GetDeviceBiometricType();
            return device;
        }
    }
}
