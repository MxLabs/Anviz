namespace Anviz.SDK.Commands
{
    class ClearNewRecordsCommand : Command
    {
        private const byte CLEAR_NEW_RECORDS = 0x4E;
        public ClearNewRecordsCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(CLEAR_NEW_RECORDS, new byte[] { 1 });
        }
    }
}
