using Anviz.SDK.Commands;
using Anviz.SDK.Utils;
using System;
using System.Threading.Tasks;

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

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Reads a fingerprint on the device and returns its template, which the device does not
        /// keep: save it on the user with SetFingerprintTemplate, as CrossChex does after an
        /// enrollment (W1, W2PRO). A finger already enrolled fails with
        /// <see cref="Responses.ResponseStatus.FingerOccupied"/>. A W1 shows the second request
        /// for a few seconds only, and then does not answer: the command times out
        /// </summary>
        /// <param name="verifyCount">How many times the user puts the finger: 1 or 2, the protocol has no more</param>
        public async Task<byte[]> EnrollFingerprint(ulong employeeID, int verifyCount = 2)
        {
            if (verifyCount < 1 || verifyCount > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(verifyCount), verifyCount, "verifyCount is 1 or 2");
            }
            var first = true;
            while (verifyCount-- > 0)
            {
                await DeviceStream.SendCommand(new EnrollFingerprintCommand(DeviceId, employeeID, first));
                first = false;
            }
            return await GetFingerprintTemplate(employeeID, 0);
        }
    }
}