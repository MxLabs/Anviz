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
        /// <summary>
        /// The id the device answers 0x46 with. <see cref="DeviceId"/> stays the one its answers
        /// carry: an M7 answers here the id set in its menu, and does not answer commands sent to it
        /// (found by level54 on an M7)
        /// </summary>
        [System.Obsolete]
        public async Task<ulong> GetDeviceID()
        {
            var response = await DeviceStream.SendCommand(new GetDeviceIDCommand(DeviceId));
            return Bytes.Read(response.DATA);
        }
    }
}