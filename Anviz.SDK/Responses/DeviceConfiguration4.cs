using Anviz.SDK.Utils;

namespace Anviz.SDK.Responses
{
    /// <summary>
    /// The firmware and protocol versions of an M4 platform device (0x36)
    /// </summary>
    public class DeviceConfiguration4
    {
        public string FirmwareVersion { get; }
        public string ProtocolVersion { get; }

        internal DeviceConfiguration4(byte[] data)
        {
            FirmwareVersion = Bytes.GetAsciiString(Bytes.Split(data, 0, 4));
            ProtocolVersion = Bytes.GetAsciiString(Bytes.Split(data, 4, 4));
        }
    }
}
