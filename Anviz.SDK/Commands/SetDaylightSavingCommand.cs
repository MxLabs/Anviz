using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetDaylightSavingCommand : Command
    {
        private const byte SET_DAYLIGHTSAVING = 0x1B;
        public SetDaylightSavingCommand(ulong deviceId, DaylightSaving value) : base(deviceId)
        {
            BuildPayload(SET_DAYLIGHTSAVING, value.ToArray());
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Sets when the device moves its clock for daylight saving time
        /// </summary>
        public async Task SetDaylightSaving(DaylightSaving value)
        {
            await DeviceStream.SendCommand(new SetDaylightSavingCommand(DeviceId, value));
        }
    }
}
