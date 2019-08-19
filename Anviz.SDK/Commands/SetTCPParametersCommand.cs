using Anviz.SDK.Responses;

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
