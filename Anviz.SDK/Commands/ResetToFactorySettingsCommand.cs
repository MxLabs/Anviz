using Anviz.SDK.Commands;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class ResetToFactorySettingsCommand : Command
    {
        private const byte DEVICE_RESET = 0x4F;
        public ResetToFactorySettingsCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(DEVICE_RESET, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task ResetToFactorySettings()
        {
            await DeviceStream.SendCommand(new ResetToFactorySettingsCommand(DeviceId));
        }
    }
}