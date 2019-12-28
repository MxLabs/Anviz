using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

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

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task SetAdvancedSettings(AdvancedSettings value)
        {
            await DeviceStream.SendCommand(new SetAdvancedSettingsCommand(DeviceId, value));
        }
    }
}