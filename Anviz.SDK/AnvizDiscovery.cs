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
        /// devices that replied within the wait, once each. A device that answers it failed is
        /// left out. On a computer with several networks (a VPN among them) 255.255.255.255 goes
        /// out of one of them only, chosen by the system: give the broadcast address of the
        /// devices' network, such as 10.255.255.255 or 192.168.1.255.
        /// </summary>
        public static Task<List<DiscoveredDevice>> Search(TimeSpan wait, IPAddress broadcast = null)
        {
            return Search(wait, new IPEndPoint(broadcast ?? IPAddress.Broadcast, DEVICE_PORT), CLIENT_PORT);
        }

        internal static async Task<List<DiscoveredDevice>> Search(TimeSpan wait, IPEndPoint to, int clientPort)
        {
            var found = new Dictionary<string, DiscoveredDevice>();
            foreach (var answer in await Broadcast(UDP_SEARCH, new byte[] { }, wait, to, clientPort, a => false))
            {
                if (answer.Ret != 0)
                {
                    continue;
                }
                var device = new DiscoveredDevice(answer.From, answer.DeviceId, answer.Data);
                found[device.MacAddress.ToString()] = device;
            }
            return found.Values.ToList();
        }

        /// <summary>
        /// Gives a device its network over UDP; it answers whether it took it. A wrong user or
        /// password is answered with a failure (W1, W2PRO: the defaults are admin and 12345, an
        /// empty user fails). Every other device that receives the setting answers that it
        /// failed, its MAC address being another: only the answer from setting.AnsweredFrom
        /// counts, or, when it is null, the first success, failing only when the wait is over.
        /// The setting turns DHCP off: it has no field for it.
        /// </summary>
        public static Task SetNetwork(UdpNetworkSetting setting, TimeSpan wait, IPAddress broadcast = null)
        {
            return SetNetwork(setting, wait, new IPEndPoint(broadcast ?? IPAddress.Broadcast, DEVICE_PORT), CLIENT_PORT);
        }

        internal static async Task SetNetwork(UdpNetworkSetting setting, TimeSpan wait, IPEndPoint to, int clientPort)
        {
            var from = setting.AnsweredFrom;
            Func<Answer, bool> counts = a => from == null || a.From.Equals(from);
            var answers = (await Broadcast(UDP_SET_NETWORK, setting.ToArray(), wait, to, clientPort,
                a => counts(a) && (from != null || a.Ret == 0))).Where(counts).ToList();
            if (answers.Any(a => a.Ret == 0))
            {
                return;
            }
            if (answers.Count == 0)
            {
                throw new TimeoutException(from == null
                    ? "No device took the network setting"
                    : $"The device at {from} did not answer the network setting");
            }
            throw new AnvizDeviceException((ResponseStatus)answers[0].Ret);
        }

        private class Answer
        {
            public IPAddress From;
            public ulong DeviceId;
            public byte Ret;
            public byte[] Data;
        }

        // Broadcast sends a command and gathers the answers until the wait is over or one is the
        // last that is needed
        private static Task<List<Answer>> Broadcast(byte command, byte[] data, TimeSpan wait,
            IPEndPoint to, int clientPort, Func<Answer, bool> last)
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
            var ack = (byte)(command + 0x80);
            // a blocking receive with a timeout: when the wait is over nothing is left listening
            return Task.Run(() => Exchange(frame, to, clientPort, ack, wait, last));
        }

        private static List<Answer> Exchange(byte[] frame, IPEndPoint to, int clientPort, byte ack,
            TimeSpan wait, Func<Answer, bool> last)
        {
            var answers = new List<Answer>();
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket.EnableBroadcast = true;
                try
                {
                    socket.Bind(new IPEndPoint(IPAddress.Any, clientPort));
                }
                catch (SocketException e) when (e.SocketErrorCode == SocketError.AccessDenied
                    || e.SocketErrorCode == SocketError.AddressAlreadyInUse)
                {
                    // the devices answer to this port only
                    throw new InvalidOperationException(
                        $"UDP port {clientPort} is taken by another program, such as CrossChex: close it", e);
                }
                socket.SendTo(frame, to);
                var buffer = new byte[2048];
                var until = DateTime.UtcNow + wait;
                while (true)
                {
                    var left = until - DateTime.UtcNow;
                    if (left <= TimeSpan.Zero)
                    {
                        break;
                    }
                    socket.ReceiveTimeout = Math.Max(1, (int)left.TotalMilliseconds);
                    EndPoint from = new IPEndPoint(IPAddress.Any, 0);
                    int length;
                    try
                    {
                        length = socket.ReceiveFrom(buffer, ref from);
                    }
                    catch (SocketException e) when (e.SocketErrorCode == SocketError.TimedOut)
                    {
                        break;
                    }
                    catch (SocketException e) when (e.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        // Windows tells a "port unreachable" of an address that did not answer
                        // this way; the others may still answer
                        continue;
                    }
                    var answer = Parse(Bytes.Split(buffer, 0, length), ack);
                    if (answer == null)
                    {
                        continue;
                    }
                    answer.From = ((IPEndPoint)from).Address;
                    answers.Add(answer);
                    if (last(answer))
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
            return new Answer
            {
                DeviceId = Bytes.Read(Bytes.Split(packet, 1, 4)),
                Ret = packet[6],
                Data = Bytes.Split(packet, 9, length)
            };
        }
    }
}
