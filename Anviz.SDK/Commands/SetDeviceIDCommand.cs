using Anviz.SDK.Utils;

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
