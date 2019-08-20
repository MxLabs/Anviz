using Anviz.SDK.Utils;

namespace Anviz.SDK.Responses
{
    public class Statistic
    {
        public ulong UserAmount { get; }
        public ulong FingerPrintAmount { get; }
        public ulong PasswordAmount { get; }
        public ulong CardAmount { get; }
        public ulong AllRecordAmount { get; }
        public ulong NewRecordAmount { get; }

        internal Statistic(byte[] data)
        {
            UserAmount = Bytes.Read(Bytes.Split(data, 0, 3));
            FingerPrintAmount = Bytes.Read(Bytes.Split(data, 3, 3));
            PasswordAmount = Bytes.Read(Bytes.Split(data, 6, 3));
            CardAmount = Bytes.Read(Bytes.Split(data, 9, 3));
            AllRecordAmount = Bytes.Read(Bytes.Split(data, 12, 3));
            NewRecordAmount = Bytes.Read(Bytes.Split(data, 15, 3));
        }

        public override string ToString()
        {
            return $"Users: {UserAmount}\r\nFingerPrints: {FingerPrintAmount}\r\nPasswords: {PasswordAmount}\r\nCards: {CardAmount}\r\nAllRecords: {AllRecordAmount}\r\nNewRecords: {NewRecordAmount}";
        }
    }
}
