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
        /// Puts the settings back as they left the factory and deletes the users and the records;
        /// the network stays as it is (W1, W2PRO). A W1 goes on answering with the old data for
        /// some seconds after the answer to this command, while it resets
        /// </summary>
        public async Task ResetToFactorySettings()
        {
            await DeviceStream.SendCommand(new ResetToFactorySettingsCommand(DeviceId));
        }
    }
}