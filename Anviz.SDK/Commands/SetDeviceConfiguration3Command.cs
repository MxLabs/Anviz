using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetDeviceConfiguration3Command : Command
    {
        private const byte SETDEVICECONFIGURATION3 = 0x35;
        public SetDeviceConfiguration3Command(ulong deviceId, DeviceConfiguration3 value) : base(deviceId)
        {
            BuildPayload(SETDEVICECONFIGURATION3, value.ToArray());
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Sets the Wiegand output, the online mode, the connection password requirement and how
        /// the door sensor is reported
        /// </summary>
        public async Task SetDeviceConfiguration3(DeviceConfiguration3 value)
        {
            await DeviceStream.SendCommand(new SetDeviceConfiguration3Command(DeviceId, value));
        }
    }
}
