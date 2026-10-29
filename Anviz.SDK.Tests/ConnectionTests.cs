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
        public async Task SilenceAfterPingsDisconnects()
        {
            // a W1 in client mode whose cable is unplugged: no more pings, and no end of the connection
            await using var fake = new FakeDevice();
            using var device = await fake.Connect();
            device.PingSilence = TimeSpan.FromSeconds(1);
            var disconnected = new TaskCompletionSource();
            device.Disconnected += (s, e) => disconnected.TrySetResult();

            await fake.Send(0x7F, Array.Empty<byte>());

            await disconnected.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(device.Connected);
        }

        [Fact]
        public async Task PingsKeepTheConnection()
        {
            await using var fake = new FakeDevice();
            using var device = await fake.Connect();
            device.PingSilence = TimeSpan.FromSeconds(1);

            for (var i = 0; i < 8; i++)
            {
                await fake.Send(0x7F, Array.Empty<byte>());
                await Task.Delay(300);
            }

            Assert.True(device.Connected);
        }

        [Fact]
        public async Task SilenceWithoutPingsKeepsTheConnection()
        {
            // a device in server mode does not ping: only commands and the keepalive tell it is gone
            await using var fake = new FakeDevice();
            using var device = await fake.Connect();
            device.PingSilence = TimeSpan.FromSeconds(1);

            await Task.Delay(2500);

            Assert.True(device.Connected);
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
        public async Task DisposeFromTheDisconnectedHandlerAndTwiceIsSafe()
        {
            await using var fake = new FakeDevice();
            var device = await fake.Connect();
            var disconnected = new TaskCompletionSource();
            device.Disconnected += (s, e) =>
            {
                device.Dispose();
                disconnected.TrySetResult();
            };

            fake.Disconnect();

            await disconnected.Task.WaitAsync(TimeSpan.FromSeconds(5));
            device.Dispose();
        }

        [Fact]
        public async Task GarbageFromThePeerClosesTheConnection()
        {
            // an HTTP server, or a scanner, on the device port
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            _ = Task.Run(async () =>
            {
                using var peer = await listener.AcceptTcpClientAsync();
                var stream = peer.GetStream();
                Assert.True(await stream.ReadAsync(new byte[64]) > 0); // the handshake command
                await stream.WriteAsync("HTTP/1.1 400 Bad Request\r\n\r\n"u8.ToArray());
                await Task.Delay(1000);
            });
            try
            {
                var ex = await Assert.ThrowsAsync<Exception>(() => new AnvizManager().Connect("127.0.0.1", port));
                Assert.Equal("Device connection lost.", ex.Message);
            }
            finally
            {
                listener.Stop();
            }
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
