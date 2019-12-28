using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

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

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task<BasicSettings> GetBasicSettings()
        {
            var response = await DeviceStream.SendCommand(new GetBasicSettingsCommand(DeviceId));
            return new BasicSettings(response.DATA);
        }
    }
}