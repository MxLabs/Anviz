using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetRebootTimesCommand : Command
    {
        private const byte GET_REBOOTTIMES = 0x14;
        public GetRebootTimesCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GET_REBOOTTIMES, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Reads the times of the day the device restarts itself
        /// </summary>
        public async Task<RebootTimes> GetRebootTimes()
        {
            var response = await DeviceStream.SendCommand(new GetRebootTimesCommand(DeviceId));
            return new RebootTimes(response.DATA);
        }
    }
}
