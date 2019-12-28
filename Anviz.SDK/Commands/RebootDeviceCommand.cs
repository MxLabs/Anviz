using Anviz.SDK.Commands;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class RebootDeviceCommand : Command
    {
        private const byte DEVICE_REBOOT = 0x2E;
        private const byte REBOOT_PARAM_UNKNOWN = 0x60;
        public RebootDeviceCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(DEVICE_REBOOT, new byte[] { REBOOT_PARAM_UNKNOWN });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task RebootDevice()
        {
            await DeviceStream.SendCommand(new RebootDeviceCommand(DeviceId));
        }
    }
}