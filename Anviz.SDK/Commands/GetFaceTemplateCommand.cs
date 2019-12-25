using Anviz.SDK.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anviz.SDK.Commands
{
    // #RDP
    class GetFaceTemplateCommand : Command
    {
        private const byte GET_FACETEMPLATE = 0x44;
        public GetFaceTemplateCommand(ulong deviceId, ulong employeeID) : base(deviceId)
        {
            var payload = new byte[6];
            Bytes.Write(5, employeeID).CopyTo(payload, 0);
            payload[5] = (byte)(1);
            BuildPayload(GET_FACETEMPLATE, payload);
        }
    }
}
