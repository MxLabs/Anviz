using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetTCPParametersCommand : Command
    {
        private const byte SET_TCP_PARAMETERS = 0x3B;
        public SetTCPParametersCommand(ulong deviceId, TcpParameters value) : base(deviceId)
        {
            BuildPayload(SET_TCP_PARAMETERS, value.ToArray());
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task SetTCPParameters(TcpParameters value)
        {
            await DeviceStream.SendCommand(new SetTCPParametersCommand(DeviceId, value));
        }
    }
}