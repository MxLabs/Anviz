using Anviz.SDK.Responses;

namespace Anviz.SDK.Tests
{
    public class TimeZoneTests
    {
        private const byte SET_TIMEZONE_INFO = 0x51;

        [Fact]
        public async Task SetSendsTheSevenDays()
        {
            await using var fake = new FakeDevice();
            fake.Handler = (cmd, data) => Task.FromResult(cmd == SET_TIMEZONE_INFO ? Reply.Ok(Array.Empty<byte>()) : fake.DefaultReply(cmd));
            using var device = await fake.Connect();
            var timeZone = new AnvizTimeZone(new byte[28]);
            timeZone.Days[AnvizTimeZone.DayName.Monday].From = new TimeSpan(8, 0, 0);
            timeZone.Days[AnvizTimeZone.DayName.Sunday].From = new TimeSpan(9, 15, 0);
            timeZone.Days[AnvizTimeZone.DayName.Sunday].To = new TimeSpan(13, 45, 0);

            await device.SetTimeZoneInfo(2, timeZone);

            // the number and Monday to Sunday, 4 bytes each: 29 bytes, as the protocol says
            var set = fake.Received.Single(r => r.Command == SET_TIMEZONE_INFO);
            Assert.Equal(29, set.Data.Length);
            Assert.Equal(2, set.Data[0]);
            Assert.Equal(new byte[] { 8, 0, 0, 0 }, set.Data[1..5]);
            Assert.Equal(new byte[] { 9, 15, 13, 45 }, set.Data[25..29]);
        }
    }
}
