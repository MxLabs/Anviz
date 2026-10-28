using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetDaylightSavingCommand : Command
    {
        private const byte GET_DAYLIGHTSAVING = 0x1A;
        public GetDaylightSavingCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GET_DAYLIGHTSAVING, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Reads when the device moves its clock for daylight saving time
        /// </summary>
        public async Task<DaylightSaving> GetDaylightSaving()
        {
            var response = await DeviceStream.SendCommand(new GetDaylightSavingCommand(DeviceId));
            return new DaylightSaving(response.DATA);
        }
    }
}
