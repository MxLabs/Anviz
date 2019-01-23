using Anviz.SDK.Utils;

namespace Anviz.SDK.Responses
{
    public class Statistic
    {
        public uint UserAmount { get; }
        public uint FingerPrintAmount { get; }
        public uint PasswordAmount { get; }
        public uint CardAmount { get; }
        public uint AllRecordAmount { get; }
        public uint NewRecordAmount { get; }

        public Statistic(byte[] data)
        {
            UserAmount = (uint)Bytes.Read(Bytes.Split(data, 0, 3));
            FingerPrintAmount = (uint)Bytes.Read(Bytes.Split(data, 3, 3));
            PasswordAmount = (uint)Bytes.Read(Bytes.Split(data, 6, 3));
            CardAmount = (uint)Bytes.Read(Bytes.Split(data, 9, 3));
            AllRecordAmount = (uint)Bytes.Read(Bytes.Split(data, 12, 3));
            NewRecordAmount = (uint)Bytes.Read(Bytes.Split(data, 15, 3));
        }

        public override string ToString()
        {
            return string.Format("User Amount : {0}\r\nFinger Print Amount : {1}\r\nPassword Amount : {2}\r\nCard Amount : {3}\r\nAll Record Amount : {4}\r\nNew Record Amount : {5}", UserAmount, FingerPrintAmount, PasswordAmount, CardAmount, AllRecordAmount, NewRecordAmount);
        }
    }
}
