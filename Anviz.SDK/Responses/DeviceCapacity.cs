using Anviz.SDK.Utils;

namespace Anviz.SDK.Responses
{
    /// <summary>
    /// How many users, fingerprints and records a device holds at most (0x5D)
    /// </summary>
    public class DeviceCapacity
    {
        public ulong MaxUsers { get; }
        public ulong MaxFingerprints { get; }
        public ulong MaxRecords { get; }

        internal DeviceCapacity(byte[] data)
        {
            MaxUsers = Bytes.Read(Bytes.Split(data, 0, 3));
            MaxFingerprints = Bytes.Read(Bytes.Split(data, 3, 3));
            MaxRecords = Bytes.Read(Bytes.Split(data, 6, 3));
        }
    }
}
