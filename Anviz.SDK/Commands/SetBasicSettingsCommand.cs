using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

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

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task SetBasicSettings(BasicSettings value)
        {
            await DeviceStream.SendCommand(new SetBasicSettingsCommand(DeviceId, value));
        }
    }
}