using Anviz.SDK.Utils;

namespace Anviz.SDK.Commands
{
    class EnrollFingerprintCommand : Command
    {
        private const byte ENROLL_FINGERPRINT = 0x5C;
        public EnrollFingerprintCommand(ulong deviceId, ulong employeeID, bool isFirst) : base(deviceId)
        {
            var payload = new byte[7];
            Bytes.Write(5, employeeID).CopyTo(payload, 0);
            payload[5] = 1;
            payload[6] = (byte)(isFirst ? 0 : 1);
            BuildPayload(ENROLL_FINGERPRINT, payload);
        }
    }
}