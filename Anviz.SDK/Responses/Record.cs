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
        /// <summary>
        /// How the user was verified, by the protocol bit 3 a card, 2 the password, 1 and 0 the
        /// fingerprints. Firmwares differ: a W1 gives 8 for a card, a W2PRO 0
        /// </summary>
        public byte BackupCode { get; set; }
        /// <summary>
        /// The attendance status in the low 4 bits (<see cref="Status"/>), bit 7 set when the
        /// record opened the door (<see cref="OpensDoor"/>)
        /// </summary>
        public byte RecordType { get; set; }
        public ulong WorkType { get; set; }

        /// <summary>
        /// The attendance status: the index of GetAttendanceStatuses or GetCustomStatuses, 0 IN, 1 OUT
        /// </summary>
        public byte Status => (byte)(RecordType & 0x0F);

        /// <summary>
        /// The record opened the door: a W2PRO, which drives the lock, sets it; a W1 with the
        /// relay on the bell does not
        /// </summary>
        public bool OpensDoor => (RecordType & 0x80) != 0;

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