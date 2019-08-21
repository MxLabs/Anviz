using Anviz.SDK.Utils;
using System;

namespace Anviz.SDK.Responses
{
    public class Record
    {
        internal const int RECORD_LENGTH = 14;
        internal const int MAX_RECORDS = 25;

        public ulong UserCode { get; set; }
        public DateTime DateTime { get; set; }
        public byte BackupCode { get; set; }
        public byte RecordType { get; set; }
        public ulong WorkType { get; set; }

        internal Record(byte[] data, int offset)
        {
            UserCode = Bytes.Read(Bytes.Split(data, offset, 5));
            var rawTime = Bytes.Read(Bytes.Split(data, offset + 5, 4));
            DateTime = DateConversions.RecordToDateTime(rawTime);
            BackupCode = data[offset + 9];
            RecordType = data[offset + 10];
            WorkType = Bytes.Read(Bytes.Split(data, offset + 11, 3));
        }

        internal byte[] ToArray()
        {
            var ret = new byte[RECORD_LENGTH];
            Bytes.Write(5, UserCode).CopyTo(ret, 0);
            var rawTime = DateConversions.DateTimeToRecord(DateTime);
            Bytes.Write(4, rawTime).CopyTo(ret, 5);
            ret[9] = BackupCode;
            ret[10] = RecordType;
            Bytes.Write(3, WorkType).CopyTo(ret, 11);
            return ret;
        }

        public Record(ulong employeeID)
        {
            UserCode = employeeID;
            DateTime = DateTime.Now;
            BackupCode = 0;
            RecordType = 0;
            WorkType = 0;
        }

        public override string ToString()
        {
            return $"UserCode: {UserCode}\r\nDateTime: {DateTime}\r\nBackupCode: {BackupCode}\r\nRecordType: {RecordType}\r\nWork Type: {WorkType}";
        }
    }
}