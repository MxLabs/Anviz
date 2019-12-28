using Anviz.SDK.Commands;
using Anviz.SDK.Utils;
using System.Threading.Tasks;

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

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task ClearNewRecords()
        {
            await DeviceStream.SendCommand(new ClearRecordsCommand(DeviceId, ClearRecordsCommand.CLEAR_ALL, 0));
        }

        public async Task ClearNewRecords(ulong amount)
        {
            await DeviceStream.SendCommand(new ClearRecordsCommand(DeviceId, ClearRecordsCommand.CLEAR_AMOUNT, amount));
        }

        public async Task DeleteAllRecords()
        {
            await DeviceStream.SendCommand(new ClearRecordsCommand(DeviceId, ClearRecordsCommand.DELETE_ALL, 0));
        }
    }
}