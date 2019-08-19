using Anviz.SDK.Utils;
using System.Collections.Generic;

namespace Anviz.SDK.Responses
{
    public class UserInfo
    {
        public ulong Id { get; }
        public ulong? Password { get; }
        public ulong? Card { get; }
        public string Name { get; }
        public List<Finger> EnrolledFingerprints { get; }
        public byte Department { get; }
        public byte Group { get; }
        public byte Mode { get; }
        public byte PWDH8 { get; }
        public byte Keep { get; }
        public byte Message { get; }

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
                Password = (ulong)(pwd[0] & 0x0F); //first 4 bytes are pwdlen
                Password = (Password << 8) | pwd[1];
                Password = (Password << 8) | pwd[2];
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

        public override string ToString()
        {
            return $"Id: {Id}\r\nName: {Name}\r\nPassword: {Password}\r\nCard: {Card}\r\nFingers: {string.Join(", ", EnrolledFingerprints)}";
        }
    }
}