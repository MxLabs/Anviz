using Anviz.SDK.Utils;
using System.Text;

namespace Anviz.SDK.Responses
{
    public class UserInfo
    {
        public ulong Id { get; }
        public string Name { get; }

        public UserInfo(byte[] data, int offset)
        {
            Id = Bytes.Read(Bytes.Split(data, offset, 5));
            Name = Encoding.BigEndianUnicode.GetString(Bytes.Split(data, offset + 12, 20)).TrimEnd('\0');
        }

        public override string ToString()
        {
            return string.Format("Id : {0}\r\nName : {1}", Id, Name);
        }
    }
}