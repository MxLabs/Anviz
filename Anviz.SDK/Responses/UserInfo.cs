using Anviz.SDK.Utils;
using System.Collections.Generic;
using System.Text;

namespace Anviz.SDK.Responses
{
    public class UserInfo
    {
        internal const int RECORD_LENGTH = 40;
        internal const int MAX_RECORDS = 8;

        public ulong Id { get; set; }
        public ulong? Password { get; set; }
        public ulong? Card { get; set; }
        public string Name { get; set; }
        public List<Finger> EnrolledFingerprints { get; }
        public byte Department { get; set; }
        public byte Group { get; set; }
        public byte Mode { get; set; }
        public byte PWDH8 { get; set; }
        public byte Keep { get; set; }
        public byte Message { get; set; }

        internal UserInfo(byte[] data, int offset)
        {
            Id = Bytes.Read(Bytes.Split(data, offset, 5));
            var pwd = Bytes.Split(data, offset + 5, 3);
            if (pwd[0] == 0xFF && pwd[1] == 0xFF && pwd[2] == 0xFF)
            {
                Password = null;
            }
            else
            {
                Password = Bytes.PasswordRead(pwd);
            }
            var card = Bytes.Split(data, offset + 8, 4);
            if (card[0] == 0xFF && card[1] == 0xFF && card[2] == 0xFF && card[3] == 0xFF)
            {
                Card = null;
            }
            else
            {
                Card = Bytes.Read(card);
            }
            Name = Bytes.GetUnicodeString(Bytes.Split(data, offset + 12, 20));
            Department = data[offset + 32];
            Group = data[offset + 33];
            Mode = data[offset + 34];
            EnrolledFingerprints = Fingers.DecodeFingers(Bytes.Read(Bytes.Split(data, offset + 35, 2)));
            PWDH8 = data[offset + 37];
            Keep = data[offset + 38];
            Message = data[offset + 39];
        }

        internal byte[] ToArray()
        {
            var ret = new byte[RECORD_LENGTH];
            Bytes.Write(5, Id).CopyTo(ret, 0);
            Bytes.PasswordWrite(Password).CopyTo(ret, 5);
            if (Card.HasValue)
            {
                Bytes.Write(4, Card.Value).CopyTo(ret, 8);
            }
            else
            {
                ret[8] = 0xFF;
                ret[9] = 0xFF;
                ret[10] = 0xFF;
                ret[11] = 0xFF;
            }
            Encoding.BigEndianUnicode.GetBytes(Name).CopyTo(ret, 12);
            ret[32] = Department;
            ret[33] = Group;
            ret[34] = Mode;
            ret[37] = PWDH8;
            ret[38] = Keep;
            ret[39] = Message;
            return ret;
        }

        public UserInfo(ulong id, string name)
        {
            Id = id;
            Name = name;
            Department = 1;
            Group = 1;
            Mode = 6;
            EnrolledFingerprints = new List<Finger>();
            PWDH8 = 0xFF;
            Keep = 0xFF;
            Message = 0x40;
        }

        public override string ToString()
        {
            return $"Id: {Id}\r\nName: {Name}\r\nPassword: {Password}\r\nCard: {Card}\r\nFingers: {string.Join(", ", EnrolledFingerprints)}";
        }
    }
}