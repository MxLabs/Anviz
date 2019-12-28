using Anviz.SDK.Commands;
using Anviz.SDK.Utils;
using System;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetDateTimeCommand : Command
    {
        private const byte GET_DATETIME = 0x38;
        public GetDateTimeCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GET_DATETIME, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task<DateTime> GetDateTime()
        {
            var cmd = new GetDateTimeCommand(DeviceId);
            var response = await DeviceStream.SendCommand(cmd);
            return DateConversions.ByteArrayToDateTime(response.DATA);
        }
    }
}