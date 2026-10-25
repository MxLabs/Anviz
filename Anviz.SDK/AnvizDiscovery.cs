using Anviz.SDK.Responses;
using Anviz.SDK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Anviz.SDK
{
    /// <summary>
    /// Finds devices and sets their network over UDP broadcast, as CrossChex does: the device
    /// listens on port 5050 and answers to port 5060. A device answers this even when its TCP
    /// address is wrong or in another network.
    /// </summary>
    public static class AnvizDiscovery
    {
        public const int DEVICE_PORT = 5050;
        public const int CLIENT_PORT = 5060;
        private const byte UDP_SEARCH = 0x02;
        private const byte UDP_SET_NETWORK = 0x03;

        /// <summary>
        /// Broadcasts a search to the given address (255.255.255.255 by default) and answers the
        /// devices that replied within the wait, once each
        /// </summary>
        public static async Task<List<DiscoveredDevice>> Search(TimeSpan wait, IPAddress broadcast = null)
        {
            var found = new Dictionary<string, DiscoveredDevice>();
            foreach (var answer in await Broadcast(UDP_SEARCH, new byte[] { }, wait, broadcast))
            {
                var device = new DiscoveredDevice(answer.From, answer.DeviceId, answer.Data);
                found[device.MacAddress.ToString()] = device;
            }
            return found.Values.ToList();
        }

        /// <summary>
        /// Gives a device its network over UDP; it answers whether it took it. A device ignores a
        /// setting with another MAC address or a wrong user or password, which then fails as a
        /// timeout.
        /// </summary>
        public static async Task SetNetwork(UdpNetworkSetting setting, TimeSpan wait, IPAddress broadcast = null)
        {
            var answers = await Broadcast(UDP_SET_NETWORK, setting.ToArray(), wait, broadcast, true);
            if (answers.Count == 0)
            {
                throw new TimeoutException("No device took the network setting");
            }
        }

        private class Answer
        {
            public IPAddress From;
            public ulong DeviceId;
            public byte[] Data;
        }

        private static async Task<List<Answer>> Broadcast(byte command, byte[] data, TimeSpan wait,
            IPAddress broadcast, bool first = false)
        {
            var frame = new byte[8 + data.Length + 2];
            frame[0] = 0xA5;
            frame[5] = command;
            frame[6] = (byte)(data.Length >> 8);
            frame[7] = (byte)(data.Length % 256);
            data.CopyTo(frame, 8);
            var crc = CRC16.Compute(frame, frame.Length - 2);
            frame[frame.Length - 2] = (byte)(crc % 256);
            frame[frame.Length - 1] = (byte)(crc >> 8);

            var answers = new List<Answer>();
            using (var client = new UdpClient())
            {
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                client.Client.Bind(new IPEndPoint(IPAddress.Any, CLIENT_PORT));
                client.EnableBroadcast = true;
                var to = new IPEndPoint(broadcast ?? IPAddress.Broadcast, DEVICE_PORT);
                await client.SendAsync(frame, frame.Length, to);
                var until = DateTime.UtcNow + wait;
                while (true)
                {
                    var left = until - DateTime.UtcNow;
                    if (left <= TimeSpan.Zero)
                    {
                        break;
                    }
                    var receiving = client.ReceiveAsync();
                    if (await Task.WhenAny(receiving, Task.Delay(left)) != receiving)
                    {
                        break;
                    }
                    var received = receiving.Result;
                    var answer = Parse(received.Buffer, (byte)(command + 0x80));
                    if (answer == null)
                    {
                        continue;
                    }
                    answer.From = received.RemoteEndPoint.Address;
                    answers.Add(answer);
                    if (first)
                    {
                        break;
                    }
                }
            }
            return answers;
        }

        // Parse reads an answer to the command, or null for a packet that is not one (another
        // command, a broken one)
        private static Answer Parse(byte[] packet, byte ack)
        {
            if (packet.Length < 11 || packet[0] != 0xA5 || packet[5] != ack)
            {
                return null;
            }
            var length = (int)Bytes.Read(Bytes.Split(packet, 7, 2));
            if (packet.Length < 9 + length + 2)
            {
                return null;
            }
            var crc = packet[9 + length] | packet[10 + length] << 8;
            if (crc != CRC16.Compute(packet, 9 + length))
            {
                return null;
            }
            if (packet[6] != 0)
            {
                throw new Exception("RET: " + packet[6]);
            }
            return new Answer
            {
                DeviceId = Bytes.Read(Bytes.Split(packet, 1, 4)),
                Data = Bytes.Split(packet, 9, length)
            };
        }
    }
}
