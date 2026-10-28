using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetLanguagesCommand : Command
    {
        private const byte GETLANGUAGES = 0x18;
        public GetLanguagesCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GETLANGUAGES, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Reads the four languages the device lets choose among
        /// </summary>
        public async Task<DeviceLanguage[]> GetLanguages()
        {
            var response = await DeviceStream.SendCommand(new GetLanguagesCommand(DeviceId));
            var languages = new DeviceLanguage[4];
            for (var i = 0; i < 4; i++)
            {
                languages[i] = (DeviceLanguage)response.DATA[i];
            }
            return languages;
        }
    }
}
