using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetStatusSwitchingCommand : Command
    {
        private const byte GETSTATUSSWITCHING = 0x20;
        public GetStatusSwitchingCommand(ulong deviceId, byte number) : base(deviceId)
        {
            BuildPayload(GETSTATUSSWITCHING, new byte[] { number });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Reads one of the 16 settings of when the device switches by itself to a status
        /// </summary>
        public async Task<StatusSwitching> GetStatusSwitching(byte number)
        {
            var response = await DeviceStream.SendCommand(new GetStatusSwitchingCommand(DeviceId, number));
            return new StatusSwitching(number, response.DATA);
        }
    }
}
