using Anviz.SDK.Utils;

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

        [Theory]
        [InlineData(Finger.RightThumb, 1)]
        [InlineData(Finger.RightIndex, 2)]
        [InlineData(Finger.LeftThumb, 10)]
        public async Task EnrollsTheFingerAskedAndReadsItsTemplate(Finger finger, byte backupCode)
        {
            await using var fake = new FakeDevice();
            fake.Handler = (cmd, data) => Task.FromResult(cmd switch
            {
                ENROLL_FINGERPRINT => Reply.Ok(Array.Empty<byte>()),
                GET_FPTEMPLATE => Reply.Ok(new byte[] { 7 }),
                _ => fake.DefaultReply(cmd),
            });
            using var device = await fake.Connect();

            await device.EnrollFingerprint(1, finger, 1);

            Assert.Equal(backupCode, fake.Received.Single(r => r.Command == ENROLL_FINGERPRINT).Data[5]);
            Assert.Equal(backupCode, fake.Received.Single(r => r.Command == GET_FPTEMPLATE).Data[5]);
        }

        [Fact]
        public async Task EnrollsTheFaceWithItsBackupCode()
        {
            var template = new byte[2056];
            await using var fake = new FakeDevice();
            fake.Handler = (cmd, data) => Task.FromResult(cmd switch
            {
                ENROLL_FINGERPRINT => Reply.Ok(Array.Empty<byte>()),
                GET_FPTEMPLATE => Reply.Ok(template),
                _ => fake.DefaultReply(cmd),
            });
            using var device = await fake.Connect();

            Assert.Equal(template, await device.EnrollFace(1));

            // 11 is the face, as for the face template
            Assert.All(fake.Received.Where(r => r.Command == ENROLL_FINGERPRINT), r => Assert.Equal(11, r.Data[5]));
            Assert.Equal(11, fake.Received.Single(r => r.Command == GET_FPTEMPLATE).Data[5]);
        }
    }
}
