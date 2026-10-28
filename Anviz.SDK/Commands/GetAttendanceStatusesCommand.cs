using Anviz.SDK.Commands;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetAttendanceStatusesCommand : Command
    {
        private const byte GETATTENDANCESTATUSES = 0x5A;
        public GetAttendanceStatusesCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GETATTENDANCESTATUSES, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Reads the attendance statuses the device offers, by index (0 in, 1 out, 2 break, up to
        /// 254): 0xFF is none
        /// </summary>
        public async Task<byte[]> GetAttendanceStatuses()
        {
            var response = await DeviceStream.SendCommand(new GetAttendanceStatusesCommand(DeviceId));
            return response.DATA;
        }
    }
}
