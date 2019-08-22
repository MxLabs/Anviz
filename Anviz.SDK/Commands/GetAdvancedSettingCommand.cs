namespace Anviz.SDK.Commands
{
    class GetAdvancedSettingsCommand : Command
    {
        private const byte GET_ADVANCED_SETTINGS = 0x32;
        public GetAdvancedSettingsCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GET_ADVANCED_SETTINGS, new byte[] { });
        }
    }
}