using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetRecordsCommand : Command
    {
        private const byte GET_RECORDS = 0x40;
        private const byte GET_ALL_RECORDS = 0x1;
        private const byte GET_NEW_RECORDS = 0x2;
        public GetRecordsCommand(ulong deviceId, bool isFirst, bool onlyNew, ulong amount) : base(deviceId)
        {
            byte recordType = onlyNew ? GET_NEW_RECORDS : GET_ALL_RECORDS;
            byte kind = isFirst ? recordType : (byte)0;
            BuildPayload(GET_RECORDS, new byte[] { kind, (byte)Math.Min(amount, Record.MAX_RECORDS) });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task<List<Record>> DownloadRecords(bool onlyNew = false)
        {
            var statistics = await GetDownloadInformation();
            var recordAmount = onlyNew ? statistics.NewRecordAmount : statistics.AllRecordAmount;
            var records = new List<Record>((int)recordAmount);
            var isFirst = true;
            while (recordAmount > 0)
            {
                var response = await DeviceStream.SendCommand(new GetRecordsCommand(DeviceId, isFirst, onlyNew, recordAmount));
                var counter = response.DATA[0];
                recordAmount -= counter;
                for (var i = 0; i < counter; i++)
                {
                    records.Add(new Record(response.DATA, 1 + i * Record.RECORD_LENGTH));
                }
                isFirst = false;
            }
            return records;
        }
    }
}