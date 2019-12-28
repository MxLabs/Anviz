using Anviz.SDK.Commands;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class UnlockDoorCommand : Command
    {
        private const byte DEVICE_UNLOCKDOOR = 0x5E;
        public UnlockDoorCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(DEVICE_UNLOCKDOOR, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task UnlockDoor()
        {
            await DeviceStream.SendCommand(new UnlockDoorCommand(DeviceId));
        }
    }
}