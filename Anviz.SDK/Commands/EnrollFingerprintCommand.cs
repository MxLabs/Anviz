using Anviz.SDK.Commands;
using Anviz.SDK.Utils;
using System;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class EnrollFingerprintCommand : Command
    {
        private const byte ENROLL_FINGERPRINT = 0x5C;
        /// <param name="backupCode">What to enroll: 1 to 10 a finger, 11 the face, as the template commands</param>
        public EnrollFingerprintCommand(ulong deviceId, ulong employeeID, byte backupCode, bool isFirst) : base(deviceId)
        {
            var payload = new byte[7];
            Bytes.Write(5, employeeID).CopyTo(payload, 0);
            payload[5] = backupCode;
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
        /// Reads the first fingerprint (<see cref="Finger.RightThumb"/>) on the device, see
        /// <see cref="EnrollFingerprint(ulong, Finger, int)"/>
        /// </summary>
        public Task<byte[]> EnrollFingerprint(ulong employeeID, int verifyCount = 2)
        {
            return EnrollFingerprint(employeeID, Finger.RightThumb, verifyCount);
        }

        /// <summary>
        /// Reads a fingerprint on the device and returns its template, which the device does not
        /// keep: save it on the user with SetFingerprintTemplate, as CrossChex does after an
        /// enrollment (W1, W2PRO). A finger already enrolled fails with
        /// <see cref="Responses.ResponseStatus.FingerOccupied"/>. A W1 shows the second request
        /// for a few seconds only, and then does not answer: the command times out
        /// </summary>
        /// <param name="verifyCount">How many times the user puts the finger: 1 or 2, the protocol has no more</param>
        public async Task<byte[]> EnrollFingerprint(ulong employeeID, Finger finger, int verifyCount = 2)
        {
            await Enroll(employeeID, (byte)(finger + 1), verifyCount);
            return await GetFingerprintTemplate(employeeID, finger);
        }

        /// <summary>
        /// Reads the face on a face device and returns its template, to save with SetFaceTemplate.
        /// It is the fingerprint enrollment with the face backup code (11), as for the face template:
        /// not tested on a face device yet. A FaceDeep 5 opens its face enrollment with this command
        /// (found by iot95); a W2PRO, without face, asks for a finger instead
        /// </summary>
        /// <param name="verifyCount">How many captures: 1 or 2</param>
        public async Task<byte[]> EnrollFace(ulong employeeID, int verifyCount = 2)
        {
            await Enroll(employeeID, GetFaceTemplateCommand.FACE_BACKUP_CODE, verifyCount);
            return await GetFaceTemplate(employeeID);
        }

        private async Task Enroll(ulong employeeID, byte backupCode, int verifyCount)
        {
            if (verifyCount < 1 || verifyCount > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(verifyCount), verifyCount, "verifyCount is 1 or 2");
            }
            var first = true;
            while (verifyCount-- > 0)
            {
                await DeviceStream.SendCommand(new EnrollFingerprintCommand(DeviceId, employeeID, backupCode, first));
                first = false;
            }
        }
    }
}
