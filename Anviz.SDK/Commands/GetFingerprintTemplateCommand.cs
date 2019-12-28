using Anviz.SDK.Commands;
using Anviz.SDK.Utils;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetFingerprintTemplateCommand : Command
    {
        private const byte GET_FPTEMPLATE = 0x44;
        public GetFingerprintTemplateCommand(ulong deviceId, ulong employeeID, Finger finger) : base(deviceId)
        {
            var payload = new byte[6];
            Bytes.Write(5, employeeID).CopyTo(payload, 0);
            payload[5] = (byte)(finger + 1);
            BuildPayload(GET_FPTEMPLATE, payload);
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task<byte[]> GetFingerprintTemplate(ulong employeeID, Finger finger)
        {
            var response = await DeviceStream.SendCommand(new GetFingerprintTemplateCommand(DeviceId, employeeID, finger));
            return response.DATA;
        }
    }
}