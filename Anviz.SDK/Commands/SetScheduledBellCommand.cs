using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetScheduledBellCommand : Command
    {
        private const byte SET_SCHEDULED_BELL = 0x55;
        public SetScheduledBellCommand(ulong deviceId, byte number, ScheduledBell value) : base(deviceId)
        {
            if (number < 0 || number > ScheduledBell.MAX_SCHEDULED_BELL_SLOT)
            {
                throw new ArgumentOutOfRangeException("number", "Must be within 0 to ScheduledBell.MAX_SCHEDULED_BELL_SLOT");
            }
            BuildPayload(SET_SCHEDULED_BELL, value.ToArray(number));
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Sets one of the 30 bells GetScheduledBells reads. The protocol does not say where the
        /// numbers start, and firmwares differ: a W2PRO takes 1 to 30 for the bells 0 to 29 and
        /// ignores 0, a W1 takes 0 to 29 and ignores 30. Read the bells back to know
        /// </summary>
        public async Task SetScheduledBell(byte number, ScheduledBell value)
        {
            await DeviceStream.SendCommand(new SetScheduledBellCommand(DeviceId, number, value));
        }
    }
}