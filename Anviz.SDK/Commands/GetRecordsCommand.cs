using System;

namespace Anviz.SDK.Commands
{
    class GetRecordsCommand : Command
    {
        private const int MAX_RECORDS = 25;
        private const byte GET_RECORDS = 0x40;
        private const byte GET_ALL_RECORDS = 0x1;
        private const byte GET_NEW_RECORDS = 0x2;
        public GetRecordsCommand(ulong deviceId, bool isFirst, bool onlyNew, uint amount) : base(deviceId)
        {
            byte recordType = onlyNew ? GET_NEW_RECORDS : GET_ALL_RECORDS;
            byte kind = isFirst ? recordType : (byte)0;
            BuildPayload(GET_RECORDS, new byte[] { kind, (byte)Math.Min(amount, MAX_RECORDS) });
        }
    }
}
