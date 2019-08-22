using Anviz.SDK.Responses;
using System;

namespace Anviz.SDK.Commands
{
    class GetStaffDataCommand : Command
    {
        private const byte GET_STAFF_DATA = 0x72;
        public GetStaffDataCommand(ulong deviceId, bool isFirst, ulong amount) : base(deviceId)
        {
            var kind = (byte)(isFirst ? 1 : 0);
            BuildPayload(GET_STAFF_DATA, new byte[] { kind, (byte)Math.Min(amount, UserInfo.MAX_RECORDS) });
        }
    }
}
