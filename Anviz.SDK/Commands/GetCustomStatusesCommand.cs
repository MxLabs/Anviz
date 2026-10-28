using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetCustomStatusesCommand : Command
    {
        private const byte GETCUSTOMSTATUSES = 0x70;
        public GetCustomStatusesCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GETCUSTOMSTATUSES, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Reads the attendance statuses the device offers by name
        /// </summary>
        public async Task<CustomStatuses> GetCustomStatuses()
        {
            var response = await DeviceStream.SendCommand(new GetCustomStatusesCommand(DeviceId));
            return new CustomStatuses(response.DATA);
        }
    }
}
