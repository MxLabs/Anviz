using Anviz.SDK.Utils;

namespace Anviz.SDK.Commands
{
    class SetFingerprintTemplateCommand : Command
    {
        private const byte SET_FPTEMPLATE = 0x45;
        public SetFingerprintTemplateCommand(ulong deviceId, ulong employeeID, Finger finger, byte[] template) : base(deviceId)
        {
            var payload = new byte[344];
            Bytes.Write(5, employeeID).CopyTo(payload, 0);
            payload[5] = (byte)(finger + 1);
            template.CopyTo(payload, 6);
            BuildPayload(SET_FPTEMPLATE, payload);
        }
    }
}