using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetInformationCodeCommand : Command
    {
        private const byte GETINFORMATIONCODE = 0x4A;
        public GetInformationCodeCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GETINFORMATIONCODE, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Reads the text the device shows as its maker or owner
        /// </summary>
        public async Task<InformationCode> GetInformationCode()
        {
            var response = await DeviceStream.SendCommand(new GetInformationCodeCommand(DeviceId));
            return new InformationCode(response.DATA);
        }
    }
}
