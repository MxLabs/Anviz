using System.Net;
using System.Net.Sockets;

namespace Anviz.SDK.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public async Task DisconnectedIsRaisedWhenTheDeviceCloses()
        {
            await using var fake = new FakeDevice();
            using var device = await fake.Connect();
            var disconnected = new TaskCompletionSource();
            device.Disconnected += (s, e) => disconnected.TrySetResult();
            Assert.True(device.Connected);

            fake.Disconnect();

            await disconnected.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(device.Connected);
        }

        [Fact]
        public async Task DisconnectedIsRaisedOnceOnDispose()
        {
            await using var fake = new FakeDevice();
            var device = await fake.Connect();
            var count = 0;
            var disconnected = new TaskCompletionSource();
            device.Disconnected += (s, e) =>
            {
                Interlocked.Increment(ref count);
                disconnected.TrySetResult();
            };

            device.Dispose();
            fake.Disconnect();

            await disconnected.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await Task.Delay(100);
            Assert.Equal(1, count);
            Assert.False(device.Connected);
        }

        [Fact]
        public async Task FakeDeviceInClientModeConnectsToTheManager()
        {
            var port = FreePort();
            var manager = new AnvizManager();
            manager.Listen(new IPEndPoint(IPAddress.Loopback, port));
            try
            {
                await using var fake = FakeDevice.ConnectTo(port, "CLIENT01");
                using var device = await manager.Accept();

                Assert.Equal("CLIENT01", await device.GetDeviceSN());
            }
            finally
            {
                manager.StopServer();
            }
        }

        [Fact]
        public async Task FakeDeviceTakesANewConnection()
        {
            await using var fake = new FakeDevice();
            using (var first = await fake.Connect())
            {
                Assert.Equal("FAKE0001", await first.GetDeviceSN());
            }

            using var second = await fake.Connect();

            Assert.Equal("FAKE0001", await second.GetDeviceSN());
            Assert.Equal(2, fake.Connections);
        }

        internal static int FreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
