using Anviz.SDK.Utils;

namespace Anviz.SDK.Responses
{
    public class UserInfo
    {
        public ulong Id { get; }
        public byte[] PWD { get; }
        public byte[] Card { get; }
        public string Name { get; }
        public byte Department { get; }
        public byte Group { get; }
        public byte Mode { get; }
        public byte[] FP { get; }
        public byte PWDH8 { get; }
        public byte Keep { get; }
        public byte Message { get; }

        public UserInfo(byte[] data, int offset)
        {
            Id = Bytes.Read(Bytes.Split(data, offset, 5));
            PWD = Bytes.Split(data, offset + 6, 3);
            Card = Bytes.Split(data, offset + 9, 3);
            Name = Bytes.GetUnicodeString(Bytes.Split(data, offset + 12, 20));
            Department = data[ offset + 32];
            Group = data[ offset + 33];
            Mode = data[offset + 34];
            FP = Bytes.Split(data, offset + 35, 2);
            PWDH8 = data[offset + 37];
            Keep = data[offset + 38];
            Message = data[offset + 39];
        }

        public override string ToString()
        {
            return $"Id: {Id}\r\nName: {Name}";
        }
    }
}