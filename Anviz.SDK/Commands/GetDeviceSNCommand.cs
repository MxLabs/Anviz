using Anviz.SDK.Commands;
using Anviz.SDK.Utils;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetDeviceSNCommand : Command
    {
        private const byte GET_DEVICE_SN = 0x24;
        public GetDeviceSNCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GET_DEVICE_SN, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task<string> GetDeviceSN()
        {
            var response = await DeviceStream.SendCommand(new GetDeviceSNCommand(DeviceId));
            return Bytes.GetAsciiString(response.DATA);
        }
    }
}