using Anviz.SDK.Utils;
using System;

namespace Anviz.SDK.Responses
{
    /// <summary>
    /// A change made to a user on the device (0x64): their data, their fingerprints, or the user
    /// deleted
    /// </summary>
    public class PersonnelChange
    {
        public const int RECORD_LENGTH = 10;
        public const int MAX_RECORDS = 40;

        public ulong UserId { get; }
        public DateTime At { get; }
        public bool UserChanged { get; }
        public bool FingerprintsChanged { get; }
        public bool UserDeleted { get; }

        internal PersonnelChange(byte[] data, int offset)
        {
            UserId = Bytes.Read(Bytes.Split(data, offset, 5));
            At = new DateTime(2000, 1, 1).AddSeconds(Bytes.Read(Bytes.Split(data, offset + 5, 4)));
            var type = data[offset + 9];
            UserChanged = (type & 1) != 0;
            FingerprintsChanged = (type & 2) != 0;
            UserDeleted = (type & 4) != 0;
        }
    }
}
