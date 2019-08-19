namespace Anviz.SDK.Commands
{
    class ResetToFactorySettingsCommand : Command
    {
        private const byte DEVICE_RESET = 0x4F;
        public ResetToFactorySettingsCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(DEVICE_RESET, new byte[] { });
        }
    }
}
