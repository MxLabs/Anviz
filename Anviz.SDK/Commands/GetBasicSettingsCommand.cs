namespace Anviz.SDK.Commands
{
    class GetBasicSettingsCommand : Command
    {
        private const byte GET_BASIC_SETTINGS = 0x30;
        public GetBasicSettingsCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GET_BASIC_SETTINGS, new byte[] { });
        }
    }
}