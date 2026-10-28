using Anviz.SDK.Responses;

namespace Anviz.SDK.Tests
{
    public class AnvizStreamTests
    {
        private const byte GET_DEVICE_SN = 0x24;
        private const byte GET_DATETIME = 0x38;
        private const byte GET_FACETEMPLATE = 0x44;

        [Fact]
        public async Task ReadsAnswersSplitInSingleBytes()
        {
            await using var fake = new FakeDevice { SplitAnswers = true };
            fake.Handler = (cmd, data) => Task.FromResult(cmd == GET_DEVICE_SN ? Reply.Ok("SN123") : FakeDevice.DefaultReply(cmd));
            using var device = await fake.Connect();

            Assert.Equal("SN123", await device.GetDeviceSN());
        }

        [Fact]
        public async Task GetsAnswersThatArriveImmediately()
        {
            await using var fake = new FakeDevice();
            fake.Handler = (cmd, data) => Task.FromResult(cmd == GET_DEVICE_SN ? Reply.Ok("SN123") : FakeDevice.DefaultReply(cmd));
            using var device = await fake.Connect();

            for (var i = 0; i < 1000; i++)
            {
                Assert.Equal("SN123", await device.GetDeviceSN());
            }
        }

        [Fact]
        public async Task SendsConcurrentCommandsOneAtATime()
        {
            var inFlight = 0;
            var maxInFlight = 0;
            await using var fake = new FakeDevice();
            fake.Handler = async (cmd, data) =>
            {
                if (cmd != GET_DEVICE_SN)
                {
                    return FakeDevice.DefaultReply(cmd);
                }
                var now = Interlocked.Increment(ref inFlight);
                InterlockedMax(ref maxInFlight, now);
                await Task.Delay(Random.Shared.Next(0, 3));
                Interlocked.Decrement(ref inFlight);
                return Reply.Ok("SN123");
            };
            using var device = await fake.Connect();

            var results = await Task.WhenAll(Enumerable.Range(0, 50).Select(_ => device.GetDeviceSN()));

            Assert.All(results, sn => Assert.Equal("SN123", sn));
            Assert.Equal(1, maxInFlight);
        }

        [Fact]
        public async Task DeviceErrorThrowsAndKeepsTheConnection()
        {
            await using var fake = new FakeDevice();
            fake.Handler = (cmd, data) => Task.FromResult(cmd switch
            {
                GET_DATETIME => Reply.Error(ResponseStatus.Fail),
                GET_DEVICE_SN => Reply.Ok("SN123"),
                _ => FakeDevice.DefaultReply(cmd),
            });
            using var device = await fake.Connect();

            var ex = await Assert.ThrowsAsync<AnvizDeviceException>(() => device.GetDateTime());
            Assert.Equal(ResponseStatus.Fail, ex.Status);
            Assert.Equal("SN123", await device.GetDeviceSN());
        }

        [Fact]
        public async Task ReadsPacketsBiggerThan16000Bytes()
        {
            var template = Enumerable.Range(0, 20000).Select(i => (byte)i).ToArray();
            await using var fake = new FakeDevice();
            fake.Handler = (cmd, data) => Task.FromResult(cmd == GET_FACETEMPLATE ? Reply.Ok(template) : FakeDevice.DefaultReply(cmd));
            using var device = await fake.Connect();

            Assert.Equal(template, await device.GetFaceTemplate(1));
        }

        [Fact]
        public async Task AnswersPingWithPong()
        {
            await using var fake = new FakeDevice();
            using var device = await fake.Connect();
            var pinged = new TaskCompletionSource();
            device.DevicePing += (s, e) => pinged.TrySetResult();

            await fake.Send(0x7F, Array.Empty<byte>());

            await pinged.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await WaitUntil(() => fake.Received.Any(r => r.Command == 0xFF));
        }

        [Fact]
        public async Task DisconnectedDeviceFailsTheCommandAndTheNextOnes()
        {
            await using var fake = new FakeDevice();
            fake.Handler = (cmd, data) =>
            {
                if (cmd == GET_DEVICE_SN)
                {
                    fake.Disconnect();
                    return Task.FromResult<Reply?>(null);
                }
                return Task.FromResult(FakeDevice.DefaultReply(cmd));
            };
            using var device = await fake.Connect();

            var lost = await Assert.ThrowsAsync<Exception>(() => device.GetDeviceSN());
            Assert.Equal("Device connection lost.", lost.Message);
            var notConnected = await Assert.ThrowsAsync<Exception>(() => device.GetDeviceSN());
            Assert.Equal("Device is not connected.", notConnected.Message);
        }

        private static void InterlockedMax(ref int target, int value)
        {
            int current;
            while ((current = Volatile.Read(ref target)) < value
                && Interlocked.CompareExchange(ref target, value, current) != current)
            {
            }
        }

        private static async Task WaitUntil(Func<bool> condition)
        {
            var timeout = DateTime.UtcNow.AddSeconds(5);
            while (!condition())
            {
                Assert.True(DateTime.UtcNow < timeout, "condition not met in 5 seconds");
                await Task.Delay(10);
            }
        }
    }
}
