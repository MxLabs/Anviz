using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetRecordsCommand : Command
    {
        private const byte SET_RECORDS = 0x41;
        public SetRecordsCommand(ulong deviceId, Record record) : base(deviceId)
        {
            BuildPayload(SET_RECORDS, record.ToArray());
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task SetRecords(Record record)
        {
            await DeviceStream.SendCommand(new SetRecordsCommand(DeviceId, record));
        }
    }
}