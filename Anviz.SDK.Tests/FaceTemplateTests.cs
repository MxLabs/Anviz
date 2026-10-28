namespace Anviz.SDK.Tests
{
    public class FaceTemplateTests
    {
        private const byte GET_FACETEMPLATE = 0x44;
        private const byte SET_FACETEMPLATE = 0x45;

        /// <summary>
        /// Reads a capture: hex bytes separated by spaces, lines starting with # are comments
        /// </summary>
        private static byte[] Capture(string name)
        {
            var lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "Captures", name));
            return lines
                .Where(l => !l.StartsWith('#'))
                .SelectMany(l => l.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Select(h => Convert.ToByte(h, 16))
                .ToArray();
        }

        [Fact]
        public async Task SetSendsThePacketCrossChexSends()
        {
            var capture = Capture("issue53-set-face-template.hex");
            var template = capture[14..^2]; // after STX, CH, CMD, LEN, user code and backup code; before CRC
            await using var fake = new FakeDevice();
            fake.Handler = (cmd, data) => Task.FromResult(cmd == SET_FACETEMPLATE ? Reply.Ok(Array.Empty<byte>()) : FakeDevice.DefaultReply(cmd));
            using var device = await fake.Connect();

            await device.SetFaceTemplate(1, template);

            Assert.Equal(capture, fake.ReceivedPackets.Last());
        }

        [Fact]
        public async Task GetAsksForTheFace()
        {
            var template = new byte[2056];
            await using var fake = new FakeDevice();
            fake.Handler = (cmd, data) => Task.FromResult(cmd == GET_FACETEMPLATE ? Reply.Ok(template) : FakeDevice.DefaultReply(cmd));
            using var device = await fake.Connect();

            Assert.Equal(template, await device.GetFaceTemplate(1));
            var get = fake.Received.Single(r => r.Command == GET_FACETEMPLATE);
            Assert.Equal(new byte[] { 0, 0, 0, 0, 1, 11 }, get.Data);
        }
    }
}
