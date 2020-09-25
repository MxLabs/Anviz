namespace Anviz.SDK.Commands
{
    internal class PongCommand : Command
    {
        private const byte DEVICE_PONG = 0xFF;
        public PongCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(DEVICE_PONG, new byte[] { });
        }
    }
}