using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetTimeZoneInfoCommand : Command
    {
        private const byte SET_TIMEZONE_INFO = 0x51;
        public SetTimeZoneInfoCommand(ulong deviceId, byte number, AnvizTimeZone value) : base(deviceId)
        {
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