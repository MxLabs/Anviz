using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

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

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task<AdvancedSettings> GetAdvancedSettings()
        {
            var response = await DeviceStream.SendCommand(new GetAdvancedSettingsCommand(DeviceId));
            return new AdvancedSettings(response.DATA);
        }
    }
}