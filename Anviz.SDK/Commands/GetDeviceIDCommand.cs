using Anviz.SDK.Commands;
using Anviz.SDK.Utils;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetDeviceIDCommand : Command
    {
        private const byte GET_DEVICE_ID = 0x46;
        public GetDeviceIDCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GET_DEVICE_ID, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        [System.Obsolete]
        public async Task<ulong> GetDeviceID()
        {
            var response = await DeviceStream.SendCommand(new GetDeviceIDCommand(DeviceId));
            DeviceId = Bytes.Read(response.DATA);
            return DeviceId;
        }
    }
}