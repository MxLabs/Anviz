namespace Anviz.SDK.Commands
{
    class GetDeviceIDCommand : Command
    {
        private const byte GET_DEVICE_ID = 0x46;
        public GetDeviceIDCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GET_DEVICE_ID, new byte[] { });
        }
    }
}
