namespace Anviz.SDK.Commands
{
    class GetDeviceTypeCommand : Command
    {
        private const byte GET_DEVICE_TYPE = 0x48;
        public GetDeviceTypeCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GET_DEVICE_TYPE, new byte[] { });
        }
    }
}
