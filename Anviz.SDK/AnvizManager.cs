using Anviz.SDK.Events;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Anviz.SDK
{
    public class AnvizManager
    {
        private TcpListener server;

        public string ConnectionUser { get; set; } = "admin";
        public string ConnectionPassword { get; set; } = "12345";
        public bool AuthenticateConnection { get; set; } = false;

        private Thread deviceAccepter = null;
        public Dictionary<string, AnvizDevice> ConnectedDevices { get; } = new Dictionary<string, AnvizDevice>();
        public event EventHandler<DeviceConnectedEventArgs> DeviceConnected;
        public event EventHandler<DeviceLostEventArgs> DeviceLost;

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

        public void StartAutoAccept()
        {
            if (server == null)
            {
                throw new Exception("Server must be started first");
            }
            if (deviceAccepter != null)
            {
                throw new Exception("AutoAccept already started");
            }
            deviceAccepter = new Thread(AutoAccept)
            {
                IsBackground = true
            };
            deviceAccepter.Start();
        }

        private async void AutoAccept()
        {
            while (server != null)
            {
                AnvizDevice incoming;
                try
                {
                    incoming = await Accept();
                }
                catch (Exception)
                {
                    break;
                }
                var serial = await incoming.GetDeviceSN();
                incoming.DeviceError += (o, e) =>
                {
                    ConnectedDevices.Remove(serial);
                    DeviceLost?.Invoke(this, new DeviceLostEventArgs(serial));
                };
                /* TODOs:
                 * handle external errors
                 * handle reconnections same serial
                 * handle missing heartbeat
                 * disconnect on own errors
                 */
                ConnectedDevices.Add(serial, incoming);
                DeviceConnected?.Invoke(this, new DeviceConnectedEventArgs(serial, incoming));
            }
            deviceAccepter = null;
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
