using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetExtendedInfoCommand : Command
    {
        private const byte SET_EXTENDEDINFO = 0x7B;
        public SetExtendedInfoCommand(ulong deviceId, ExtendedInfo value) : base(deviceId)
        {
            BuildPayload(SET_EXTENDEDINFO, value.ToArray());
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Sets the vendor's name, address and tax number the device keeps
        /// </summary>
        public async Task SetExtendedInfo(ExtendedInfo value)
        {
            await DeviceStream.SendCommand(new SetExtendedInfoCommand(DeviceId, value));
        }
    }
}
