using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetRecordInfoCommand : Command
    {
        private const byte GET_RECORD_INFO = 0x3C;
        public GetRecordInfoCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GET_RECORD_INFO, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task<Statistic> GetDownloadInformation()
        {
            var response = await DeviceStream.SendCommand(new GetRecordInfoCommand(DeviceId));
            return new Statistic(response.DATA);
        }
    }
}