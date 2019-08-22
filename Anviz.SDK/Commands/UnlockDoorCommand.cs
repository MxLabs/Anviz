namespace Anviz.SDK.Commands
{
    class UnlockDoorCommand : Command
    {
        private const byte DEVICE_UNLOCKDOOR = 0x5E;
        public UnlockDoorCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(DEVICE_UNLOCKDOOR, new byte[] { });
        }
    }
}
