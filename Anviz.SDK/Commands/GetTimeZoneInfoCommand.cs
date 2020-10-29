using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetTimeZoneInfoCommand : Command
    {
        private const byte GET_TIMEZONE_INFO = 0x50;
        public GetTimeZoneInfoCommand(ulong deviceId, byte number) : base(deviceId)
        {
            if (number < 1 || number > AnvizTimeZone.MAX_TIMEZONE_SLOT)
            {
                throw new ArgumentOutOfRangeException("number", "Must be within 1 to AnvizTimeZone.MAX_TIMEZONE_SLOT");
            }
            BuildPayload(GET_TIMEZONE_INFO, new byte[] { number });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task<AnvizTimeZone> GetTimeZoneInfo(byte number)
        {
            var response = await DeviceStream.SendCommand(new GetTimeZoneInfoCommand(DeviceId, number));
            return new AnvizTimeZone(response.DATA);
        }
    }
}