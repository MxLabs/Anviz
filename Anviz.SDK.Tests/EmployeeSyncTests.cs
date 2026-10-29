using Anviz.SDK.Responses;
using Anviz.SDK.Utils;

namespace Anviz.SDK.Tests
{
    public class EmployeeSyncTests
    {
        private const byte GET_STAFF_DATA = 0x72;
        private const byte SET_STAFF_DATA = 0x73;
        private const byte DELETE_STAFF_DATA = 0x4C;
        private const byte DELETE_STAFF_DATA_ALL = 0x4D;
        private const byte SET_FPTEMPLATE = 0x45;

        private static byte[] Template(byte seed) => Enumerable.Range(0, 338).Select(i => (byte)(i + seed)).ToArray();

        private static EmployeeData Employee(ulong id, string name, params Finger[] fingers)
        {
            var employee = new EmployeeData(new UserInfo(id, name) { Card = 1000 + id });
            foreach (var finger in fingers)
            {
                employee.Fingerprints[finger] = Template((byte)(id * 10 + (ulong)finger));
            }
            return employee;
        }

        private static List<EmployeeData> Staff() => new List<EmployeeData>
        {
            Employee(1, "Anna", Finger.RightIndex),
            Employee(2, "Bruno", Finger.RightIndex, Finger.LeftIndex),
            Employee(3, "Carla"),
        };

        [Fact]
        public async Task EmptyDeviceGetsEverything()
        {
            await using var fake = new FakeDevice();
            var store = new FakeEmployeeStore(fake);
            using var device = await fake.Connect();

            var result = await device.SyncEmployees(Staff());

            Assert.Equal(3, result.Added);
            Assert.Equal(3, result.FingerprintsWritten);
            Assert.Equal(new ulong[] { 1, 2, 3 }, store.Ids);
            Assert.Equal("Bruno", store.User(2).Name);
            Assert.Equal(1002UL, store.User(2).Card);
            Assert.Equal(Template(2 * 10 + (byte)Finger.LeftIndex), store.Template(2, Finger.LeftIndex));
            Assert.Equal(new[] { Finger.RightIndex, Finger.LeftIndex }, store.User(2).EnrolledFingerprints);
        }

        [Fact]
        public async Task SecondSyncSendsNoEmployeeAndRewritesTheTemplates()
        {
            await using var fake = new FakeDevice();
            var store = new FakeEmployeeStore(fake);
            using var device = await fake.Connect();
            await device.SyncEmployees(Staff());
            store.Commands.Clear();

            var result = await device.SyncEmployees(Staff());

            Assert.Equal(3, result.Unchanged);
            Assert.Equal(0, result.Added + result.Updated + result.Recreated + result.Deleted);
            Assert.False(store.Commands.ContainsKey(SET_STAFF_DATA));
            Assert.False(store.Commands.ContainsKey(DELETE_STAFF_DATA));
            Assert.Equal(3, store.Commands[SET_FPTEMPLATE]);
        }

        [Fact]
        public async Task OnlyChangedEmployeesAreSent()
        {
            await using var fake = new FakeDevice();
            var store = new FakeEmployeeStore(fake);
            using var device = await fake.Connect();
            await device.SyncEmployees(Staff());
            store.Commands.Clear();
            var staff = Staff();
            staff[1].User.Name = "Bruna";

            var result = await device.SyncEmployees(staff);

            Assert.Equal(1, result.Updated);
            Assert.Equal(2, result.Unchanged);
            Assert.Equal(1, store.Commands[SET_STAFF_DATA]);
            Assert.Equal("Bruna", store.User(2).Name);
            Assert.Equal(new[] { Finger.RightIndex, Finger.LeftIndex }, store.User(2).EnrolledFingerprints);
        }

        [Fact]
        public async Task EmployeesNotWantedAreDeleted()
        {
            await using var fake = new FakeDevice();
            var store = new FakeEmployeeStore(fake);
            store.Add(new UserInfo(99, "Old"), Finger.RightThumb);
            using var device = await fake.Connect();

            var result = await device.SyncEmployees(Staff());

            Assert.Equal(1, result.Deleted);
            Assert.Equal(new ulong[] { 1, 2, 3 }, store.Ids);
            Assert.Null(store.Template(99, Finger.RightThumb));
        }

        [Fact]
        public async Task NoEmployeesClearsTheDeviceWithOneCommand()
        {
            await using var fake = new FakeDevice();
            var store = new FakeEmployeeStore(fake);
            for (ulong id = 1; id <= 20; id++)
            {
                store.Add(new UserInfo(id, "U" + id));
            }
            using var device = await fake.Connect();

            var result = await device.SyncEmployees(new List<EmployeeData>());

            Assert.Equal(20, result.Deleted);
            Assert.Empty(store.Ids);
            Assert.Equal(1, store.Commands[DELETE_STAFF_DATA_ALL]);
            Assert.False(store.Commands.ContainsKey(DELETE_STAFF_DATA));
        }

        [Fact]
        public async Task EmployeeWithAFingerNotWantedIsRecreated()
        {
            await using var fake = new FakeDevice();
            var store = new FakeEmployeeStore(fake);
            store.Add(new UserInfo(1, "Anna") { Card = 1001 }, Finger.RightIndex, Finger.RightMiddle);
            using var device = await fake.Connect();

            var result = await device.SyncEmployees(new[] { Employee(1, "Anna", Finger.RightIndex) });

            Assert.Equal(1, result.Recreated);
            Assert.Equal(new[] { Finger.RightIndex }, store.User(1).EnrolledFingerprints);
            Assert.Null(store.Template(1, Finger.RightMiddle));
            Assert.Equal(Template(10 + (byte)Finger.RightIndex), store.Template(1, Finger.RightIndex));
        }

        [Fact]
        public async Task SameEmployeeTwiceIsRejected()
        {
            await using var fake = new FakeDevice();
            _ = new FakeEmployeeStore(fake);
            using var device = await fake.Connect();

            await Assert.ThrowsAsync<ArgumentException>(() => device.SyncEmployees(new[] { Employee(1, "A"), Employee(1, "B") }));
        }

        [Fact]
        public async Task ManyDevicesAreSynchronizedInParallel()
        {
            var writing = 0;
            var maxWriting = 0;
            void OnTemplate(bool start)
            {
                if (!start)
                {
                    Interlocked.Decrement(ref writing);
                    return;
                }
                var now = Interlocked.Increment(ref writing);
                int max;
                while ((max = Volatile.Read(ref maxWriting)) < now && Interlocked.CompareExchange(ref maxWriting, now, max) != max)
                {
                }
            }
            var fakes = Enumerable.Range(0, 4).Select(i => new FakeDevice { Serial = "PAR" + i }).ToList();
            var stores = fakes.Select(f => new FakeEmployeeStore(f) { TemplateDelay = TimeSpan.FromMilliseconds(30), OnTemplate = OnTemplate }).ToList();
            var devices = new List<AnvizDevice>();
            try
            {
                foreach (var fake in fakes)
                {
                    devices.Add(await fake.Connect());
                }

                var results = await AnvizSync.SyncEmployees(devices, Staff());

                Assert.All(results.Values, r => Assert.Null(r.Error));
                Assert.All(results.Values, r => Assert.Equal(3, r.FingerprintsWritten));
                Assert.All(stores, s => Assert.Equal(new ulong[] { 1, 2, 3 }, s.Ids));
                Assert.True(maxWriting > 1, $"devices written one at a time ({maxWriting})");
            }
            finally
            {
                devices.ForEach(d => d.Dispose());
                foreach (var fake in fakes)
                {
                    await fake.DisposeAsync();
                }
            }
        }

        [Fact]
        public async Task MaxParallelLimitsTheDevicesAtOnce()
        {
            var writing = 0;
            var maxWriting = 0;
            void OnTemplate(bool start)
            {
                var now = start ? Interlocked.Increment(ref writing) : Interlocked.Decrement(ref writing);
                if (start && now > Volatile.Read(ref maxWriting))
                {
                    Volatile.Write(ref maxWriting, now);
                }
            }
            var fakes = Enumerable.Range(0, 3).Select(i => new FakeDevice { Serial = "ONE" + i }).ToList();
            fakes.ForEach(f => _ = new FakeEmployeeStore(f) { TemplateDelay = TimeSpan.FromMilliseconds(20), OnTemplate = OnTemplate });
            var devices = new List<AnvizDevice>();
            try
            {
                foreach (var fake in fakes)
                {
                    devices.Add(await fake.Connect());
                }

                await AnvizSync.SyncEmployees(devices, Staff(), maxParallel: 1);

                Assert.Equal(1, maxWriting);
            }
            finally
            {
                devices.ForEach(d => d.Dispose());
                foreach (var fake in fakes)
                {
                    await fake.DisposeAsync();
                }
            }
        }

        [Fact]
        public async Task AFailingDeviceDoesNotStopTheOthers()
        {
            await using var good = new FakeDevice { Serial = "GOOD01" };
            await using var bad = new FakeDevice { Serial = "BAD001" };
            var goodStore = new FakeEmployeeStore(good);
            _ = new FakeEmployeeStore(bad) { FailTemplates = true };
            using var goodDevice = await good.Connect();
            using var badDevice = await bad.Connect();

            var results = await AnvizSync.SyncEmployees(new[] { goodDevice, badDevice }, Staff());

            Assert.Null(results[goodDevice].Error);
            var error = Assert.IsType<AnvizDeviceException>(results[badDevice].Error);
            Assert.Equal(ResponseStatus.Fail, error.Status);
            Assert.Equal(new ulong[] { 1, 2, 3 }, goodStore.Ids);
            Assert.True(badDevice.Connected);
        }

        [Fact]
        public async Task ProgressGoesThroughTheSteps()
        {
            await using var fake = new FakeDevice();
            _ = new FakeEmployeeStore(fake);
            using var device = await fake.Connect();
            var steps = new List<SyncStep>();

            await device.SyncEmployees(Staff(), progress: new SyncProgressRecorder(steps));

            Assert.Equal(new[] { SyncStep.ReadingEmployees, SyncStep.WritingEmployees, SyncStep.WritingFingerprints, SyncStep.Done }, steps.Distinct());
        }

        [Fact]
        public async Task WipeDeletesAllAndWritesEverythingWithoutReading()
        {
            await using var fake = new FakeDevice();
            var store = new FakeEmployeeStore(fake);
            using var device = await fake.Connect();
            await device.SyncEmployees(Staff());
            store.Add(new UserInfo(9, "Extra"), Finger.RightThumb);
            store.Commands.Clear();
            var steps = new List<SyncStep>();

            var result = await device.SyncEmployees(Staff(), SyncMode.Wipe, new SyncProgressRecorder(steps));

            Assert.Equal(3, result.Added);
            Assert.Equal(0, result.Updated + result.Recreated + result.Deleted + result.Unchanged);
            Assert.Equal(3, result.FingerprintsWritten);
            Assert.Equal(1, store.Commands[DELETE_STAFF_DATA_ALL]);
            Assert.False(store.Commands.ContainsKey(GET_STAFF_DATA));
            Assert.False(store.Commands.ContainsKey(DELETE_STAFF_DATA));
            Assert.Equal(1, store.Commands[SET_STAFF_DATA]);
            Assert.Equal(new ulong[] { 1, 2, 3 }, store.Ids);
            Assert.Equal(new[] { Finger.RightIndex, Finger.LeftIndex }, store.User(2).EnrolledFingerprints);
            Assert.Equal(new[] { SyncStep.DeletingEmployees, SyncStep.WritingEmployees, SyncStep.WritingFingerprints, SyncStep.Done }, steps.Distinct());
        }

        [Fact]
        public async Task WipeOnManyDevices()
        {
            var fakes = new List<FakeDevice>();
            var devices = new List<AnvizDevice>();
            try
            {
                for (var i = 0; i < 3; i++)
                {
                    var fake = new FakeDevice();
                    var store = new FakeEmployeeStore(fake);
                    store.Add(new UserInfo(9, "Extra"));
                    fakes.Add(fake);
                    devices.Add(await fake.Connect());
                }

                var results = await AnvizSync.SyncEmployees(devices, Staff(), SyncMode.Wipe);

                Assert.All(results.Values, r => Assert.Equal(3, r.Added));
                Assert.All(results.Values, r => Assert.Null(r.Error));
            }
            finally
            {
                devices.ForEach(d => d.Dispose());
                foreach (var fake in fakes)
                {
                    await fake.DisposeAsync();
                }
            }
        }

        [Fact]
        public async Task SetEmployeesDataLeavesTheListAsItWas()
        {
            await using var fake = new FakeDevice();
            _ = new FakeEmployeeStore(fake);
            using var device = await fake.Connect();
            var users = Enumerable.Range(1, 10).Select(i => new UserInfo((ulong)i, "U" + i)).ToList();

            await device.SetEmployeesData(users);

            Assert.Equal(10, users.Count);
        }

        /// <summary>
        /// Records synchronously: Progress&lt;T&gt; would report on the thread pool, after the call
        /// </summary>
        private sealed class SyncProgressRecorder : IProgress<SyncProgress>
        {
            private readonly List<SyncStep> steps;
            public SyncProgressRecorder(List<SyncStep> steps) => this.steps = steps;
            public void Report(SyncProgress value)
            {
                lock (steps)
                {
                    steps.Add(value.Step);
                }
            }
        }
    }
}
