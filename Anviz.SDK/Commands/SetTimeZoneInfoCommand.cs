using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetTimeZoneInfoCommand : Command
    {
        private const byte SET_TIMEZONE_INFO = 0x51;
        public SetTimeZoneInfoCommand(ulong deviceId, byte number, AnvizTimeZone value) : base(deviceId)
        {
            if (number < 1 || number > AnvizTimeZone.MAX_TIMEZONE_SLOT)
            {
                throw new ArgumentOutOfRangeException("number", "Must be within 1 to AnvizTimeZone.MAX_TIMEZONE_SLOT");
            }
            BuildPayload(SET_TIMEZONE_INFO, value.ToArray(number));
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task SetTimeZoneInfo(byte number, AnvizTimeZone value)
        {
            await DeviceStream.SendCommand(new SetTimeZoneInfoCommand(DeviceId, number, value));
        }
    }
}