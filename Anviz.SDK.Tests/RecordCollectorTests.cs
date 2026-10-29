using Anviz.SDK.Events;
using System.Collections.Concurrent;
using System.Net;
using Record = Anviz.SDK.Responses.Record;

namespace Anviz.SDK.Tests
{
    public class RecordCollectorTests
    {
        private const byte SET_ADVANCED_SETTINGS = 0x33;
        private const byte DELETE_RECORDS = 0x4E;
        private static readonly TimeSpan WAIT = TimeSpan.FromSeconds(5);
        private static readonly DateTime At = new DateTime(2026, 10, 29, 8, 0, 0);

        /// <summary>
        /// A store that keeps what it takes, answering what Answer says (true by default)
        /// </summary>
        private sealed class Store
        {
            public ConcurrentQueue<RecordBatch> Batches { get; } = new();
            public ConcurrentQueue<RecordBatch> Taken { get; } = new();
            public Func<RecordBatch, bool> Answer { get; set; } = b => true;

            public Task<bool> Take(RecordBatch batch, CancellationToken token)
            {
                Batches.Enqueue(batch);
                var taken = Answer(batch);
                if (taken)
                {
                    Taken.Enqueue(batch);
                }
                return Task.FromResult(taken);
            }

            public List<(ulong, DateTime)> Records => Taken.SelectMany(b => b.Records).Select(r => (r.UserCode, r.DateTime)).ToList();
        }

        private static AnvizRecordCollector Collector(Store store) => new AnvizRecordCollector(store.Take)
        {
            PollInterval = TimeSpan.FromSeconds(30),
            RetryDelay = TimeSpan.FromMilliseconds(200),
        };

        private static async Task WaitUntil(Func<bool> condition)
        {
            var until = DateTime.UtcNow + WAIT;
            while (!condition())
            {
                Assert.True(DateTime.UtcNow < until, "condition not met in time");
                await Task.Delay(10);
            }
        }

        [Fact]
        public async Task KeptRecordsAreStoredAndDeleted()
        {
            await using var fake = new FakeDevice();
            var device = new FakeRecordStore(fake);
            device.Keep(1, At);
            device.Keep(2, At.AddMinutes(1));
            using var anviz = await fake.Connect();
            var store = new Store();
            using var collector = Collector(store);

            collector.Add("FAKE0001", anviz);

            await WaitUntil(() => device.Count == 0);
            var batch = Assert.Single(store.Taken);
            Assert.Equal(RecordSource.Download, batch.Source);
            Assert.Equal("FAKE0001", batch.Serial);
            Assert.Equal(new[] { (1UL, At), (2UL, At.AddMinutes(1)) }, store.Records);
        }

        [Fact]
        public async Task RealTimeIsTurnedOn()
        {
            await using var fake = new FakeDevice();
            var device = new FakeRecordStore(fake);
            using var anviz = await fake.Connect();
            using var collector = Collector(new Store());

            collector.Add("FAKE0001", anviz);

            await WaitUntil(() => device.RealTimeMode);
        }

        [Fact]
        public async Task RealTimeIsLeftAsItIsWhenNotEnabled()
        {
            await using var fake = new FakeDevice();
            var device = new FakeRecordStore(fake);
            device.Keep(1, At);
            using var anviz = await fake.Connect();
            using var collector = Collector(new Store());
            collector.EnableRealTime = false;

            collector.Add("FAKE0001", anviz);

            await WaitUntil(() => device.Count == 0);
            Assert.False(device.RealTimeMode);
            Assert.False(device.Commands.ContainsKey(SET_ADVANCED_SETTINGS));
        }

        [Fact]
        public async Task ARecordSentTwiceInRealTimeIsStoredOnceAndDeleted()
        {
            await using var fake = new FakeDevice();
            var device = new FakeRecordStore(fake) { RealTimeMode = true };
            using var anviz = await fake.Connect();
            var store = new Store();
            using var collector = Collector(store);
            collector.Add("FAKE0001", anviz);
            await WaitUntil(() => device.Commands.ContainsKey(0x3C));

            // a W2PRO sends it twice
            await device.Verify(1, At, sent: 2);

            await WaitUntil(() => device.Count == 0 && !store.Taken.IsEmpty);
            await Task.Delay(300);
            var batch = Assert.Single(store.Taken);
            Assert.Equal(RecordSource.RealTime, batch.Source);
            Assert.Equal(new[] { (1UL, At) }, store.Records);
        }

        [Fact]
        public async Task RecordsStayOnTheDeviceUntilTheStoreTakesThem()
        {
            await using var fake = new FakeDevice();
            var device = new FakeRecordStore(fake) { RealTimeMode = true };
            device.Keep(1, At);
            using var anviz = await fake.Connect();
            var store = new Store { Answer = b => false };
            using var collector = Collector(store);
            collector.Add("FAKE0001", anviz);
            await WaitUntil(() => store.Batches.Count >= 2);

            // made while the store does not take anything
            await device.Verify(2, At.AddMinutes(1));
            await Task.Delay(500);
            Assert.Equal(2, device.Count);
            Assert.False(device.Commands.ContainsKey(DELETE_RECORDS));

            store.Answer = b => true;

            await WaitUntil(() => device.Count == 0);
            Assert.Equal(new[] { (1UL, At), (2UL, At.AddMinutes(1)) }, store.Records.OrderBy(r => r.Item1));
        }

        [Fact]
        public async Task ARecordMadeWhileDownloadingIsNotDeleted()
        {
            await using var fake = new FakeDevice();
            var device = new FakeRecordStore(fake);
            device.Keep(1, At);
            var made = 0;
            // made without real time, between the download and the deletion
            device.OnDownload = () =>
            {
                if (Interlocked.Exchange(ref made, 1) == 0)
                {
                    Task.Run(() => device.Keep(2, At.AddMinutes(1)));
                }
            };
            using var anviz = await fake.Connect();
            var store = new Store();
            using var collector = Collector(store);
            collector.EnableRealTime = false;

            collector.Add("FAKE0001", anviz);

            await WaitUntil(() => device.Count == 0 && store.Records.Count == 2);
            Assert.Equal(new[] { (1UL, At), (2UL, At.AddMinutes(1)) }, store.Records.OrderBy(r => r.Item1));
        }

        [Fact]
        public async Task ThePollFindsRecordsNotSentInRealTime()
        {
            await using var fake = new FakeDevice();
            var device = new FakeRecordStore(fake);
            using var anviz = await fake.Connect();
            var store = new Store();
            using var collector = Collector(store);
            collector.EnableRealTime = false;
            collector.PollInterval = TimeSpan.FromMilliseconds(200);
            collector.Add("FAKE0001", anviz);
            await WaitUntil(() => device.Commands.ContainsKey(0x3C));

            device.Keep(1, At);

            await WaitUntil(() => device.Count == 0 && store.Records.Count == 1);
            Assert.Equal(RecordSource.Download, store.Taken.Single().Source);
        }

        [Fact]
        public async Task AStoreThatThrowsIsReportedAndRetried()
        {
            await using var fake = new FakeDevice();
            var device = new FakeRecordStore(fake);
            device.Keep(1, At);
            using var anviz = await fake.Connect();
            var calls = 0;
            var store = new Store { Answer = b => Interlocked.Increment(ref calls) == 1 ? throw new InvalidOperationException("db down") : true };
            using var collector = Collector(store);
            var errors = new ConcurrentQueue<RecordCollectorErrorEventArgs>();
            collector.Error += (s, e) => errors.Enqueue(e);

            collector.Add("FAKE0001", anviz);

            await WaitUntil(() => device.Count == 0);
            var error = Assert.Single(errors);
            Assert.Equal("db down", error.Exception.Message);
            Assert.Equal("FAKE0001", error.Serial);
            Assert.Equal(new[] { (1UL, At) }, store.Records);
        }

        [Fact]
        public async Task RemovedDeviceKeepsItsRecords()
        {
            await using var fake = new FakeDevice();
            var device = new FakeRecordStore(fake) { RealTimeMode = true };
            using var anviz = await fake.Connect();
            var store = new Store();
            using var collector = Collector(store);
            collector.Add("FAKE0001", anviz);
            await WaitUntil(() => device.Commands.ContainsKey(0x3C));

            Assert.True(collector.Remove("FAKE0001"));
            await device.Verify(1, At);
            await Task.Delay(300);

            Assert.Equal(1, device.Count);
            Assert.Empty(store.Batches);
            Assert.False(collector.Remove("FAKE0001"));
        }

        [Fact]
        public async Task CollectsFromTheDevicesOfAServer()
        {
            var server = new AnvizServer { ReconnectDelay = TimeSpan.FromMilliseconds(100), HandshakeTimeout = TimeSpan.FromSeconds(2) };
            server.Listen(new IPEndPoint(IPAddress.Loopback, 0));
            var store = new Store();
            using var collector = new AnvizRecordCollector(server, store.Take) { RetryDelay = TimeSpan.FromMilliseconds(200) };
            using var cts = new CancellationTokenSource();
            var run = server.RunAsync(cts.Token);
            try
            {
                await using var first = FakeDevice.ConnectTo(server.LocalEndpoints[0].Port, "DEV1");
                var firstRecords = new FakeRecordStore(first);
                firstRecords.Keep(1, At);
                await using var second = FakeDevice.ConnectTo(server.LocalEndpoints[0].Port, "DEV2");
                var secondRecords = new FakeRecordStore(second);
                secondRecords.Keep(2, At);

                await WaitUntil(() => firstRecords.Count == 0 && secondRecords.Count == 0 && store.Taken.Count == 2);
                Assert.Equal(new[] { "DEV1", "DEV2" }, store.Taken.Select(b => b.Serial).OrderBy(s => s));

                // disconnected, a record made meanwhile, and connected again
                Assert.True(server.Disconnect("DEV1"));
                await WaitUntil(() => !server.Serials.Contains("DEV1"));
                await using var again = FakeDevice.ConnectTo(server.LocalEndpoints[0].Port, "DEV1");
                var againRecords = new FakeRecordStore(again);
                againRecords.Keep(3, At);
                await WaitUntil(() => againRecords.Count == 0 && store.Records.Contains((3UL, At)));
            }
            finally
            {
                cts.Cancel();
                await run.WaitAsync(WAIT);
                server.Dispose();
            }
        }
    }
}
