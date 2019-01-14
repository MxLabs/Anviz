using System;

namespace Anviz.SDK.Commands
{
    class GetStaffDataCommand : Command
    {
        private const int MAX_RECORDS = 8;
        private const byte GET_STAFF_DATA = 0x72;
        public GetStaffDataCommand(ulong deviceId, bool isFirst, uint amount) : base(deviceId)
        {
            byte kind = (byte)(isFirst ? 1 : 0);
            BuildPayload(GET_STAFF_DATA, new byte[] { kind, (byte)Math.Min(amount, MAX_RECORDS) });
        }
    }
}
