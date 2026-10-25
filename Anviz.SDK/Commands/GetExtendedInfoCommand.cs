using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetExtendedInfoCommand : Command
    {
        private const byte GET_EXTENDEDINFO = 0x7A;
        public GetExtendedInfoCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GET_EXTENDEDINFO, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Reads the vendor's name, address and tax number the device keeps
        /// </summary>
        public async Task<ExtendedInfo> GetExtendedInfo()
        {
            var response = await DeviceStream.SendCommand(new GetExtendedInfoCommand(DeviceId));
            return new ExtendedInfo(response.DATA);
        }
    }
}
