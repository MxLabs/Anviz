using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetScheduledBellsCommand : Command
    {
        private const byte GET_SCHEDULED_BELL = 0x54;
        public GetScheduledBellsCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GET_SCHEDULED_BELL, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task<List<ScheduledBell>> GetScheduledBells()
        {
            var response = await DeviceStream.SendCommand(new GetScheduledBellsCommand(DeviceId));
            var bells = new List<ScheduledBell>(ScheduledBell.MAX_SCHEDULED_BELL_SLOT);
            var counter = 0;
            while (counter < response.DATA.Length)
            {
                bells.Add(new ScheduledBell(response.DATA, counter));
                counter += ScheduledBell.RECORD_LENGTH;
            }
            return bells;
        }
    }
}