using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetCustomStatusesCommand : Command
    {
        private const byte SETCUSTOMSTATUSES = 0x71;
        public SetCustomStatusesCommand(ulong deviceId, CustomStatuses value) : base(deviceId)
        {
            BuildPayload(SETCUSTOMSTATUSES, value.ToArray());
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Sets the attendance statuses the device offers by name; it then shows these instead
        /// of the list of indexes (0x5B)
        /// </summary>
        public async Task SetCustomStatuses(CustomStatuses value)
        {
            await DeviceStream.SendCommand(new SetCustomStatusesCommand(DeviceId, value));
        }
    }
}
