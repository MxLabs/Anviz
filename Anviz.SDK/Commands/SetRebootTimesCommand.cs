using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetRebootTimesCommand : Command
    {
        private const byte SET_REBOOTTIMES = 0x15;
        public SetRebootTimesCommand(ulong deviceId, RebootTimes value) : base(deviceId)
        {
            BuildPayload(SET_REBOOTTIMES, value.ToArray());
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Sets the times of the day the device restarts itself
        /// </summary>
        public async Task SetRebootTimes(RebootTimes value)
        {
            await DeviceStream.SendCommand(new SetRebootTimesCommand(DeviceId, value));
        }
    }
}
