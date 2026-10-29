namespace Anviz.SDK.Tests
{
    public class EnrollFingerprintTests
    {
        private const byte ENROLL_FINGERPRINT = 0x5C;
        private const byte GET_FPTEMPLATE = 0x44;

        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        public async Task RejectsVerifyCountOtherThan1Or2(int verifyCount)
        {
            await using var fake = new FakeDevice();
            using var device = await fake.Connect();

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => device.EnrollFingerprint(1, verifyCount));
        }

        [Fact]
        public async Task EnrollsTwiceThenReadsTheTemplate()
        {
            var template = new byte[] { 1, 2, 3 };
            await using var fake = new FakeDevice();
            fake.Handler = (cmd, data) => Task.FromResult(cmd switch
            {
                ENROLL_FINGERPRINT => Reply.Ok(Array.Empty<byte>()),
                GET_FPTEMPLATE => Reply.Ok(template),
                _ => fake.DefaultReply(cmd),
            });
            using var device = await fake.Connect();

            Assert.Equal(template, await device.EnrollFingerprint(1, 2));

            var enrolls = fake.Received.Where(r => r.Command == ENROLL_FINGERPRINT).ToList();
            Assert.Equal(2, enrolls.Count);
            Assert.Equal(0, enrolls[0].Data[6]); // first time
            Assert.Equal(1, enrolls[1].Data[6]); // second time
        }
    }
}
