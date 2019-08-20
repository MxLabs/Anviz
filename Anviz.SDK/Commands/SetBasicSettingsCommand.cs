using Anviz.SDK.Responses;

namespace Anviz.SDK.Commands
{
    class SetBasicSettingsCommand : Command
    {
        private const byte SET_BASIC_SETTINGS = 0x31;
        public SetBasicSettingsCommand(ulong deviceId, BasicSettings value) : base(deviceId)
        {
            BuildPayload(SET_BASIC_SETTINGS, value.ToArray());
        }
    }
}
