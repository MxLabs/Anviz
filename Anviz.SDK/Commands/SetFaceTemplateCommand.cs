using Anviz.SDK.Commands;
using Anviz.SDK.Utils;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetFaceTemplateCommand : Command
    {
        private const byte SET_FACETEMPLATE = 0x45;
        public SetFaceTemplateCommand(ulong deviceId, ulong employeeID, byte[] template) : base(deviceId)
        {
            var payload = new byte[15366];
            Bytes.Write(5, employeeID).CopyTo(payload, 0);
            payload[5] = 1;
            template.CopyTo(payload, 6);
            BuildPayload(SET_FACETEMPLATE, payload);
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task SetFaceTemplate(ulong employeeID, byte[] template)
        {
            await DeviceStream.SendCommand(new SetFaceTemplateCommand(DeviceId, employeeID, template));
        }
    }
}