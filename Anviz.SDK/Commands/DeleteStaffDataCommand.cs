using Anviz.SDK.Utils;

namespace Anviz.SDK.Commands
{
    class DeleteStaffDataCommand : Command
    {
        private const byte DELETE_STAFF_DATA = 0x4C;
        private const byte DELETE_FULL = 0xFF;
        public DeleteStaffDataCommand(ulong deviceId, ulong employeeID) : base(deviceId)
        {
            var payload = new byte[6];
            Bytes.Write(5, employeeID).CopyTo(payload, 0);
            payload[5] = DELETE_FULL;
            BuildPayload(DELETE_STAFF_DATA, payload);
        }
    }
}
