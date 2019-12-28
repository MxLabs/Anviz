using Anviz.SDK.Commands;
using Anviz.SDK.Utils;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetDeviceIDCommand : Command
    {
        private const byte SET_DEVICE_ID = 0x47;
        public SetDeviceIDCommand(ulong deviceId, ulong newDeviceId) : base(deviceId)
        {
            BuildPayload(SET_DEVICE_ID, Bytes.Write(4, newDeviceId));
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task SetDeviceID(ulong newDeviceId)
        {
            await DeviceStream.SendCommand(new SetDeviceIDCommand(DeviceId, newDeviceId));
            if (DeviceId != 0)
            {
                DeviceId = newDeviceId;
            }
        }
    }
}