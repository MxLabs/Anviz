using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetDeviceConfiguration3Command : Command
    {
        private const byte GETDEVICECONFIGURATION3 = 0x34;
        public GetDeviceConfiguration3Command(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GETDEVICECONFIGURATION3, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Reads the Wiegand output, the online mode, the connection password requirement and how
        /// the door sensor is reported
        /// </summary>
        public async Task<DeviceConfiguration3> GetDeviceConfiguration3()
        {
            var response = await DeviceStream.SendCommand(new GetDeviceConfiguration3Command(DeviceId));
            return new DeviceConfiguration3(response.DATA);
        }
    }
}
