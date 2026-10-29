using Anviz.SDK.Responses;

namespace Anviz.SDK.Tests
{
    /// <summary>
    /// What an M7 answers, found by level54: a flag per employee written, and an id of its menu
    /// </summary>
    public class StaffAnswerTests
    {
        private const byte SET_STAFF_DATA = 0x73;
        private const byte GET_DEVICE_ID = 0x46;
        private const byte GET_DEVICE_SN = 0x24;

        private static async Task<(FakeDevice, AnvizDevice)> Device(byte[] staffAnswer)
        {
            var fake = new FakeDevice();
            fake.Handler = (cmd, data) => Task.FromResult(cmd == SET_STAFF_DATA ? Reply.Ok(staffAnswer) : fake.DefaultReply(cmd));
            return (fake, await fake.Connect());
        }

        private static List<UserInfo> Users(int count) => Enumerable.Range(1, count).Select(i => new UserInfo((ulong)i, "U" + i)).ToList();

        [Theory]
        [InlineData(new byte[] { 0x0F, 0xFF })] // a W1 and a W2PRO, whatever was stored
        [InlineData(new byte[] { 0x00, 0x07 })] // the three sent, stored
        [InlineData(new byte[] { })]             // no flag
        public async Task StoredEmployeesAreFine(byte[] answer)
        {
            var (fake, device) = await Device(answer);
            await using (fake)
            using (device)
            {
                await device.SetEmployeesData(Users(3));
            }
        }

        [Fact]
        public async Task AnEmployeeNotStoredThrows()
        {
            // the second of three not stored
            var (fake, device) = await Device(new byte[] { 0x00, 0x05 });
            await using (fake)
            using (device)
            {
                var failed = await Assert.ThrowsAsync<AnvizDeviceException>(() => device.SetEmployeesData(Users(3)));
                Assert.Equal(ResponseStatus.Fail, failed.Status);
                Assert.True(device.Connected);
            }
        }

        [Fact]
        public async Task AFullCommandNeedsEightBits()
        {
            // the eighth of a command of 8 not stored
            var (fake, device) = await Device(new byte[] { 0x00, 0x7F });
            await using (fake)
            using (device)
            {
                await device.SetEmployeesData(Users(7));
                await Assert.ThrowsAsync<AnvizDeviceException>(() => device.SetEmployeesData(Users(8)));
            }
        }

#pragma warning disable CS0612 // GetDeviceID is obsolete
        [Fact]
        public async Task GetDeviceIdLeavesTheIdTheDeviceAnswersWith()
        {
            // an M7 answers 0x46 with the id of its menu, and nothing to commands sent to it
            await using var fake = new FakeDevice();
            fake.Handler = (cmd, data) => Task.FromResult(cmd switch
            {
                GET_DEVICE_ID => Reply.Ok(new byte[] { 0x31, 0x35, 0x39, 0x30 }),
                GET_DEVICE_SN => Reply.Ok("SN123"),
                _ => fake.DefaultReply(cmd),
            });
            using var device = await fake.Connect();

            Assert.Equal(0x31353930UL, await device.GetDeviceID());

            Assert.Equal(FakeDevice.DEVICE_ID, device.DeviceId);
            Assert.Equal("SN123", await device.GetDeviceSN());
        }
#pragma warning restore CS0612
    }
}
