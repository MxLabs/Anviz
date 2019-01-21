using Anviz.SDK.Utils;

namespace Anviz.SDK.Responses
{
    public class Record
    {
        public ulong UserCode { get; }
        public ulong DateTime { get; }
        public byte BackupCode { get; }
        public byte RecordType { get; }
        public uint WorkType { get; }

        public Record(byte[] data, int offset)
        {
            UserCode = Bytes.Read(Bytes.Split(data, offset, 5));
            DateTime = Bytes.Read(Bytes.Split(data, offset + 5, 4));
            BackupCode = data[offset + 10];
            RecordType = data[offset + 11];
            WorkType = (uint)Bytes.Read(Bytes.Split(data, offset + 12, 3));
        }

        public override string ToString()
        {
            return string.Format("User Code : {0}\r\nDate Time : {1}\r\nBackup Code : {2}\r\nRecord Type : {3}\r\nWork Type : {4}", UserCode, DateTime, BackupCode, RecordType, WorkType);
        }
    }
}