using Anviz.SDK.Commands;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class ClearAdminFlagsCommand : Command
    {
        private const byte CLEAR_ADMIN_FLAGS = 0x3D;
        public ClearAdminFlagsCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(CLEAR_ADMIN_FLAGS, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Makes every administrator of the device a normal user
        /// </summary>
        public async Task ClearAdminFlags()
        {
            await DeviceStream.SendCommand(new ClearAdminFlagsCommand(DeviceId));
        }
    }
}
