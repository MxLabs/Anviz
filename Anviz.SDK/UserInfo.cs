namespace Anviz.SDK
{
    public class UserInfo
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public UserInfo()
        {

        }

        public override string ToString()
        {
            return string.Format("Id : {0}\r\nName : {1}", Id, Name);
        }
    }
}
