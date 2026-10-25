using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetServerUrlCommand : Command
    {
        private const byte GET_SERVERURL = 0x0A;
        public GetServerUrlCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GET_SERVERURL, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Reads the server the device calls by name in client mode, and its DNS server
        /// </summary>
        public async Task<ServerUrl> GetServerUrl()
        {
            var response = await DeviceStream.SendCommand(new GetServerUrlCommand(DeviceId));
            return new ServerUrl(response.DATA);
        }
    }
}
