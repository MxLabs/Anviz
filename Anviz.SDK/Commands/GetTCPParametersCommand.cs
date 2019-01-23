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
