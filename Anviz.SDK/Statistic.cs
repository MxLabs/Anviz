namespace Anviz.SDK
{
    public class Statistic
    {
        public uint UserAmount { get; set; }
        public uint FingerPrintAmount { get; set; }
        public uint PasswordAmount { get; set; }
        public uint CardAmount { get; set; }
        public uint AllRecordAmount { get; set; }
        public uint NewRecordAmount { get; set; }
        public Statistic()
        {

        }

        public override string ToString()
        {
            return string.Format("User Amount : {0}\r\nFinger Print Amount : {1}\r\nPassword Amount : {2}\r\nCard Amount : {3}\r\nAll Record Amount : {4}\r\nNew Record Amount : {5}", UserAmount, FingerPrintAmount, PasswordAmount, CardAmount, AllRecordAmount, NewRecordAmount);
        }
    }
}
