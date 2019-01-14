namespace Anviz.SDK.Commands
{
    class GetDeviceSNCommand : Command
    {
        private const byte GET_DEVICE_SN = 0x46;
        public GetDeviceSNCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GET_DEVICE_SN, new byte[] { });
        }
    }
}
