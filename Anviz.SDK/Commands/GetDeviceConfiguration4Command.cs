using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetDeviceConfiguration4Command : Command
    {
        private const byte GETDEVICECONFIGURATION4 = 0x36;
        public GetDeviceConfiguration4Command(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GETDEVICECONFIGURATION4, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Reads the firmware and protocol versions of an M4 platform device
        /// </summary>
        public async Task<DeviceConfiguration4> GetDeviceConfiguration4()
        {
            var response = await DeviceStream.SendCommand(new GetDeviceConfiguration4Command(DeviceId));
            return new DeviceConfiguration4(response.DATA);
        }
    }
}
