using Anviz.SDK.Utils;
using System;

namespace Anviz.SDK.Responses
{
    public class Record
    {
        public ulong UserCode { get; }
        public DateTime DateTime { get; }
        public byte BackupCode { get; }
        public byte RecordType { get; }
        public uint WorkType { get; }

        public Record(byte[] data, int offset)
        {
            UserCode = Bytes.Read(Bytes.Split(data, offset, 5));
            var rawTime = Bytes.Read(Bytes.Split(data, offset + 5, 4));
            DateTime = DateConversions.RecordToDateTime(rawTime);
            BackupCode = data[offset + 9];
            RecordType = data[offset + 10];
            WorkType = (uint)Bytes.Read(Bytes.Split(data, offset + 11, 3));
        }

        public override string ToString()
        {
            return $"UserCode: {UserCode}\r\nDateTime: {DateTime}\r\nBackupCode: {BackupCode}\r\nRecordType: {RecordType}\r\nWork Type: {WorkType}";
        }
    }
}