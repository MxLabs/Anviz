using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetInformationCodeCommand : Command
    {
        private const byte SETINFORMATIONCODE = 0x4B;
        public SetInformationCodeCommand(ulong deviceId, InformationCode value) : base(deviceId)
        {
            BuildPayload(SETINFORMATIONCODE, value.ToArray());
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Sets the text the device shows as its maker or owner
        /// </summary>
        public async Task SetInformationCode(InformationCode value)
        {
            await DeviceStream.SendCommand(new SetInformationCodeCommand(DeviceId, value));
        }
    }
}
