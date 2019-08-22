using Anviz.SDK.Utils;

namespace Anviz.SDK.Commands
{
    class ClearRecordsCommand : Command
    {
        internal const byte DELETE_ALL = 0x00;
        internal const byte CLEAR_ALL = 0x01;
        internal const byte CLEAR_AMOUNT = 0x02;
        private const byte DELETE_RECORDS = 0x4E;
        public ClearRecordsCommand(ulong deviceId, byte deleteMode, ulong deleteAmount) : base(deviceId)
        {
            var ret = new byte[4];
            ret[0] = deleteMode;
            Bytes.Write(3, deleteAmount).CopyTo(ret, 1);
            BuildPayload(DELETE_RECORDS, ret);
        }
    }
}
