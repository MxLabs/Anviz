namespace Anviz.SDK.Commands
{
    class RebootDeviceCommand : Command
    {
        private const byte DEVICE_REBOOT = 0x2E;
        private const byte REBOOT_PARAM_UNKNOWN = 0x60;
        public RebootDeviceCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(DEVICE_REBOOT, new byte[] { REBOOT_PARAM_UNKNOWN });
        }
    }
}
