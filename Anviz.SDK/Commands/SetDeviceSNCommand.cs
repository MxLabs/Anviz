using System.Text;

namespace Anviz.SDK.Commands
{
    class SetDeviceSNCommand : Command
    {
        private const byte SET_DEVICE_SN = 0x25;
        public SetDeviceSNCommand(ulong deviceId, string value) : base(deviceId)
        {
            var payload = new byte[16];
            Encoding.ASCII.GetBytes(value).CopyTo(payload, 0);
            BuildPayload(SET_DEVICE_SN, payload);
        }
    }
}
