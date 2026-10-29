using Anviz.SDK.Responses;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Anviz.SDK.Tests
{
    public class DiscoveryTests
    {
        private const byte SET_NETWORK_ACK = 0x83;
        private static readonly IPAddress Device = IPAddress.Parse("127.0.0.1");
        private static readonly IPAddress OtherDevice = IPAddress.Parse("127.0.0.2");

        /// <summary>
        /// Two devices on loopback that both receive a setting sent to the first, as a broadcast
        /// does: the other one answers first that it failed, its MAC being another (W1 and W2PRO)
        /// </summary>
        private sealed class TwoDevices : IDisposable
        {
            private readonly UdpClient device = new UdpClient(new IPEndPoint(Device, 0));
            private readonly UdpClient other = new UdpClient(new IPEndPoint(OtherDevice, 0));

            public IPEndPoint To => (IPEndPoint)device.Client.LocalEndPoint!;

            public TwoDevices(ResponseStatus deviceAnswer, ResponseStatus otherAnswer)
            {
                _ = Task.Run(async () =>
                {
                    var received = await device.ReceiveAsync();
                    var otherPacket = FakeDevice.Packet(SET_NETWORK_ACK, otherAnswer, Array.Empty<byte>());
                    await other.SendAsync(otherPacket, otherPacket.Length, received.RemoteEndPoint);
                    await Task.Delay(200);
                    var packet = FakeDevice.Packet(SET_NETWORK_ACK, deviceAnswer, Array.Empty<byte>());
                    await device.SendAsync(packet, packet.Length, received.RemoteEndPoint);
                });
            }

            public void Dispose()
            {
                device.Dispose();
                other.Dispose();
            }
        }

        private static UdpNetworkSetting Setting(IPAddress? answeredFrom) => new UdpNetworkSetting
        {
            MacAddress = PhysicalAddress.Parse("0022CA8C1140"),
            IP = IPAddress.Parse("10.11.2.50"),
            SubnetMask = IPAddress.Parse("255.0.0.0"),
            DefaultGateway = IPAddress.Parse("10.0.0.4"),
            Server = IPAddress.Parse("192.168.0.97"),
            DeviceId = 1,
            User = "admin",
            Password = "12345",
            AnsweredFrom = answeredFrom
        };

        private static int FreePort()
        {
            using var probe = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
            return ((IPEndPoint)probe.Client.LocalEndPoint!).Port;
        }

        [Fact]
        public async Task SetNetworkTakesTheAnswerOfTheDevice()
        {
            using var devices = new TwoDevices(ResponseStatus.Success, ResponseStatus.Fail);

            await AnvizDiscovery.SetNetwork(Setting(Device), TimeSpan.FromSeconds(3), devices.To, FreePort());
        }

        [Fact]
        public async Task SetNetworkFailsAtOnceWhenTheDeviceFails()
        {
            // a wrong user or password; the other device takes it, its MAC being its own for the test
            using var devices = new TwoDevices(ResponseStatus.Fail, ResponseStatus.Success);
            var started = DateTime.UtcNow;

            var failed = await Assert.ThrowsAsync<AnvizDeviceException>(() =>
                AnvizDiscovery.SetNetwork(Setting(Device), TimeSpan.FromSeconds(5), devices.To, FreePort()));

            Assert.Equal(ResponseStatus.Fail, failed.Status);
            Assert.True(DateTime.UtcNow - started < TimeSpan.FromSeconds(3));
        }

        [Fact]
        public async Task SetNetworkWithoutTheAddressTakesTheFirstSuccess()
        {
            using var devices = new TwoDevices(ResponseStatus.Success, ResponseStatus.Fail);

            await AnvizDiscovery.SetNetwork(Setting(null), TimeSpan.FromSeconds(3), devices.To, FreePort());
        }

        [Fact]
        public async Task SetNetworkWithoutTheAddressFailsWhenNoDeviceTookIt()
        {
            using var devices = new TwoDevices(ResponseStatus.Fail, ResponseStatus.Fail);

            var failed = await Assert.ThrowsAsync<AnvizDeviceException>(() =>
                AnvizDiscovery.SetNetwork(Setting(null), TimeSpan.FromSeconds(1), devices.To, FreePort()));

            Assert.Equal(ResponseStatus.Fail, failed.Status);
        }

        [Fact]
        public async Task SearchTellsThatThePortIsTaken()
        {
            // CrossChex keeps the port the devices answer to
            using var crossChex = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            if (OperatingSystem.IsWindows())
            {
                crossChex.ExclusiveAddressUse = true;
            }
            crossChex.Bind(new IPEndPoint(IPAddress.Any, 0));
            var port = ((IPEndPoint)crossChex.LocalEndPoint!).Port;

            var taken = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                AnvizDiscovery.Search(TimeSpan.FromSeconds(1), new IPEndPoint(Device, FreePort()), port));

            Assert.Contains("CrossChex", taken.Message);
        }
    }
}
