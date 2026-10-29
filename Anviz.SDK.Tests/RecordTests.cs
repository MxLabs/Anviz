using Record = Anviz.SDK.Responses.Record;

namespace Anviz.SDK.Tests
{
    public class RecordTests
    {
        [Fact]
        public void ACardOnAW2ProOpensTheDoor()
        {
            // user 1 at 2026-10-29 16:16:33, as a W2PRO sent it in real time
            var record = new Record(new byte[] { 0, 0, 0, 0, 1, 0x32, 0x74, 0xDC, 0x61, 0, 0x80, 0, 0, 0 }, 0);

            Assert.Equal(1UL, record.UserCode);
            Assert.True(record.OpensDoor);
            Assert.Equal(0, record.Status);
            Assert.Equal(0, record.BackupCode);
        }

        [Fact]
        public void ACardOnAW1GoingOut()
        {
            // a W1 gives 8 for a card and no door; status 1 is OUT
            var record = new Record(new byte[] { 0, 0, 0, 0x23, 0x29, 0x32, 0x74, 0xDC, 0x61, 8, 0x01, 0, 0, 0 }, 0);

            Assert.Equal(9001UL, record.UserCode);
            Assert.False(record.OpensDoor);
            Assert.Equal(1, record.Status);
            Assert.Equal(8, record.BackupCode);
        }
    }
}
