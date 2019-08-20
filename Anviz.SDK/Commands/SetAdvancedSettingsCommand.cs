using Anviz.SDK.Responses;

namespace Anviz.SDK.Commands
{
    class SetAdvancedSettingsCommand : Command
    {
        private const byte SET_ADVANCED_SETTINGS = 0x33;
        public SetAdvancedSettingsCommand(ulong deviceId, AdvancedSettings value) : base(deviceId)
        {
            BuildPayload(SET_ADVANCED_SETTINGS, value.ToArray());
        }
    }
}
