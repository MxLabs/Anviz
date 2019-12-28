using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetTCPParametersCommand : Command
    {
        private const byte GET_TCP_PARAMETERS = 0x3A;
        public GetTCPParametersCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GET_TCP_PARAMETERS, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task<TcpParameters> GetTcpParameters()
        {
            var response = await DeviceStream.SendCommand(new GetTCPParametersCommand(DeviceId));
            return new TcpParameters(response.DATA);
        }
    }
}