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
