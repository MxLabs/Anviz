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
        /// <summary>
        /// Puts the settings back as they left the factory; the network stays as it is. What else
        /// goes depends on the firmware: a W2PRO deletes the users and the records too, a W1
        /// keeps them, and its vendor information and server URL
        /// </summary>
        public async Task ResetToFactorySettings()
        {
            await DeviceStream.SendCommand(new ResetToFactorySettingsCommand(DeviceId));
        }
    }
}