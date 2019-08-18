using Anviz.SDK.Utils;

namespace Anviz.SDK.Commands
{
    class GetFingerprintTemplateCommand : Command
    {
        private const byte GET_FPTEMPLATE = 0x44;
        public GetFingerprintTemplateCommand(ulong deviceId, ulong employeeID, Finger finger) : base(deviceId)
        {
            var payload = new byte[6];
            var employee = Bytes.Write(5, employeeID);
            employee.CopyTo(payload, 0);
            payload[5] = (byte)(finger + 1);
            BuildPayload(GET_FPTEMPLATE, payload);
        }
    }
}