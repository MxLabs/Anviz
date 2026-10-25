using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetStatusSwitchingCommand : Command
    {
        private const byte SETSTATUSSWITCHING = 0x21;
        public SetStatusSwitchingCommand(ulong deviceId, StatusSwitching value) : base(deviceId)
        {
            BuildPayload(SETSTATUSSWITCHING, value.ToArray());
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Sets one of the 16 settings of when the device switches by itself to a status
        /// </summary>
        public async Task SetStatusSwitching(StatusSwitching value)
        {
            await DeviceStream.SendCommand(new SetStatusSwitchingCommand(DeviceId, value));
        }
    }
}
