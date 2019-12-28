using Anviz.SDK.Utils;

namespace Anviz.SDK.Commands
{
    class GetFaceTemplateCommand : Command
    {
        private const byte GET_FACETEMPLATE = 0x44;
        public GetFaceTemplateCommand(ulong deviceId, ulong employeeID) : base(deviceId)
        {
            var payload = new byte[6];
            Bytes.Write(5, employeeID).CopyTo(payload, 0);
            payload[5] = 1;
            BuildPayload(GET_FACETEMPLATE, payload);
        }
    }
}
