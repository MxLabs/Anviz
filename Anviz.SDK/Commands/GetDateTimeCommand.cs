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
