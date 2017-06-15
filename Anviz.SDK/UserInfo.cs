using System;

namespace Anviz.SDK
{
    public class UserInfo : IComparable<UserInfo>
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public UserInfo()
        {

        }

        int IComparable<UserInfo>.CompareTo(UserInfo other)
        {
            return (int)Id - (int)other.Id;
        }

        public override string ToString()
        {
            return string.Format("Id : {0}\r\nName : {1}", Id, Name);
        }
    }
}
