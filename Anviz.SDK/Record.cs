namespace Anviz.SDK
{
    public class Record
    {
        public ulong UserCode { get; set; }
        public ulong DateTime { get; set; }
        public byte BackupCode { get; set; }
        public byte RecordType { get; set; }
        public uint WorkType { get; set; }

        public Record()
        {

        }

        public override string ToString()
        {
            return string.Format("User Code : {0}\r\nDate Time : {1}\r\nBackup Code : {2}\r\nRecord Type : {3}\r\nWork Type : {4}", UserCode, DateTime, BackupCode, RecordType, WorkType);
        }
    }
}
