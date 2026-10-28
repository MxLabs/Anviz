using Anviz.SDK.Responses;
using Anviz.SDK.Utils;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Anviz.SDK.Tests
{
    /// <summary>
    /// What the fake device answers to a command: null data with Success means no answer at all
    /// </summary>
    public record Reply(ResponseStatus Status, byte[] Data)
    {
        public static Reply Ok(byte[] data) => new Reply(ResponseStatus.Success, data);
        public static Reply Ok(string ascii) => Ok(Encoding.ASCII.GetBytes(ascii));
        public static Reply Error(ResponseStatus status) => new Reply(status, Array.Empty<byte>());
    }

    /// <summary>
    /// A device on localhost that speaks the TC_B protocol, to test the library without hardware
    /// </summary>
    public sealed class FakeDevice : IAsyncDisposable
    {
        public const ulong DEVICE_ID = 1;

        private readonly TcpListener listener;
        private readonly SemaphoreSlim writeLock = new SemaphoreSlim(1, 1);
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly TaskCompletionSource<NetworkStream> connected = new TaskCompletionSource<NetworkStream>();
        private TcpClient? client;

        public int Port => ((IPEndPoint)listener.LocalEndpoint).Port;

        /// <summary>
        /// Answers a command (code and data). The default answers the device type asked on connection
        /// </summary>
        public Func<byte, byte[], Task<Reply?>> Handler { get; set; }

        /// <summary>
        /// Writes the answers one byte at a time, as a slow network could deliver them
        /// </summary>
        public bool SplitAnswers { get; set; }

        public ConcurrentQueue<(byte Command, byte[] Data)> Received { get; } = new ConcurrentQueue<(byte, byte[])>();

        /// <summary>
        /// The whole packets received, to compare them with captures of real traffic
        /// </summary>
        public ConcurrentQueue<byte[]> ReceivedPackets { get; } = new ConcurrentQueue<byte[]>();

        public FakeDevice()
        {
            Handler = (cmd, data) => Task.FromResult(DefaultReply(cmd));
            listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            _ = Task.Run(Serve);
        }

        public static Reply? DefaultReply(byte cmd) => cmd == 0x48 ? Reply.Ok("W1\0\0\0\0\0\0") : null;

        /// <summary>
        /// Connects an AnvizDevice to this fake device
        /// </summary>
        public Task<AnvizDevice> Connect() => new AnvizManager().Connect("127.0.0.1", Port);

        /// <summary>
        /// Sends a packet the device starts by itself, like the ping (0x7F)
        /// </summary>
        public async Task Send(byte ack, byte[] data)
        {
            await Write(Packet(ack, null, data));
        }

        /// <summary>
        /// Closes the connection, as a device that is switched off
        /// </summary>
        public void Disconnect() => client?.Close();

        private async Task Serve()
        {
            try
            {
                client = await listener.AcceptTcpClientAsync(cts.Token);
                client.NoDelay = true;
                var stream = client.GetStream();
                connected.SetResult(stream);
                while (!cts.IsCancellationRequested)
                {
                    // STX + CH + CMD, then RET when it is an answer (the pong), then LEN
                    var head = await Read(stream, 6);
                    var cmd = head[5];
                    var lenBytes = await Read(stream, cmd >= 0x80 ? 3 : 2);
                    var len = (lenBytes[^2] << 8) | lenBytes[^1];
                    var data = await Read(stream, len);
                    var crc = await Read(stream, 2);
                    Received.Enqueue((cmd, data));
                    ReceivedPackets.Enqueue(head.Concat(lenBytes).Concat(data).Concat(crc).ToArray());
                    if (cmd >= 0x80)
                    {
                        continue;
                    }
                    // each command on its own: the library must match answers by code, not by order
                    _ = Task.Run(async () =>
                    {
                        var reply = await Handler(cmd, data);
                        if (reply != null)
                        {
                            await Write(Packet((byte)(cmd + 0x80), reply.Status, reply.Data));
                        }
                    });
                }
            }
            catch (Exception)
            {
                // connection closed
            }
        }

        private async Task<byte[]> Read(NetworkStream stream, int count)
        {
            var buffer = new byte[count];
            var read = 0;
            while (read < count)
            {
                var n = await stream.ReadAsync(buffer, read, count - read, cts.Token);
                if (n == 0)
                {
                    throw new EndOfStreamException();
                }
                read += n;
            }
            return buffer;
        }

        private async Task Write(byte[] packet)
        {
            var stream = await connected.Task;
            await writeLock.WaitAsync();
            try
            {
                if (!SplitAnswers)
                {
                    await stream.WriteAsync(packet);
                    return;
                }
                foreach (var b in packet)
                {
                    await stream.WriteAsync(new[] { b });
                    await stream.FlushAsync();
                    await Task.Delay(1);
                }
            }
            finally
            {
                writeLock.Release();
            }
        }

        public static byte[] Packet(byte ack, ResponseStatus? status, byte[] data)
        {
            var packet = new List<byte> { 0xA5 };
            packet.AddRange(Bytes.Write(4, DEVICE_ID));
            packet.Add(ack);
            if (status.HasValue)
            {
                packet.Add((byte)status.Value);
            }
            packet.AddRange(Bytes.Write(2, (ulong)data.Length));
            packet.AddRange(data);
            var bytes = packet.ToArray();
            var crc = CRC16.Compute(bytes, bytes.Length);
            packet.Add((byte)(crc & 0xFF));
            packet.Add((byte)(crc >> 8));
            return packet.ToArray();
        }

        public async ValueTask DisposeAsync()
        {
            cts.Cancel();
            client?.Close();
            listener.Stop();
            await Task.CompletedTask;
        }
    }
}
