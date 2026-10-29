using Anviz.SDK.Responses;
using System.Collections.Concurrent;
using Record = Anviz.SDK.Responses.Record;

namespace Anviz.SDK.Tests
{
    public class RealTimeRecordTests
    {
        private const byte REALTIME_RECORD = 0xDF;
        private const byte GET_DEVICE_SN = 0x24;

        // user 1 at 2026-10-29 16:16:33, a card on a W2PRO: the bytes a W2PRO sent twice
        private static readonly byte[] UserOne = { 0, 0, 0, 0, 1, 0x32, 0x74, 0xDC, 0x61, 0, 0x80, 0, 0, 0 };
        private static readonly byte[] UserTwo = { 0, 0, 0, 0, 2, 0x32, 0x74, 0xDC, 0x61, 0, 0x80, 0, 0, 0 };

        private static async Task<List<Record>> Receive(FakeDevice fake, AnvizDevice device, params byte[][] records)
        {
            var received = new ConcurrentQueue<Record>();
            device.ReceivedRecord += (s, r) => received.Enqueue(r);
            foreach (var record in records)
            {
                await fake.Send(REALTIME_RECORD, record, ResponseStatus.Success);
                await Task.Delay(400);
            }
            // the receiver handles packets in order: once a command is answered, the records before it are
            await device.GetDeviceSN();
            return received.ToList();
        }

        [Fact]
        public async Task ARecordSentTwiceIsReceivedOnce()
        {
            await using var fake = new FakeDevice();
            fake.Handler = (cmd, data) => Task.FromResult(cmd == GET_DEVICE_SN ? Reply.Ok("SN123") : fake.DefaultReply(cmd));
            using var device = await fake.Connect();

            var records = await Receive(fake, device, UserOne, UserOne);

            var record = Assert.Single(records);
            Assert.Equal(1UL, record.UserCode);
        }

        [Fact]
        public async Task DifferentRecordsAreAllReceived()
        {
            await using var fake = new FakeDevice();
            fake.Handler = (cmd, data) => Task.FromResult(cmd == GET_DEVICE_SN ? Reply.Ok("SN123") : fake.DefaultReply(cmd));
            using var device = await fake.Connect();

            var records = await Receive(fake, device, UserOne, UserTwo, UserOne);

            Assert.Equal(new ulong[] { 1, 2, 1 }, records.Select(r => r.UserCode));
        }
    }
}
