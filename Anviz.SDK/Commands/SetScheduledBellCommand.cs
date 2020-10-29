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
        public async Task SetScheduledBell(byte number, ScheduledBell value)
        {
            await DeviceStream.SendCommand(new SetScheduledBellCommand(DeviceId, number, value));
        }
    }
}