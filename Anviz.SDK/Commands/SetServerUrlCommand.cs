using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetServerUrlCommand : Command
    {
        private const byte SET_SERVERURL = 0x0B;
        public SetServerUrlCommand(ulong deviceId, ServerUrl value) : base(deviceId)
        {
            BuildPayload(SET_SERVERURL, value.ToArray());
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Sets the server the device calls by name in client mode, and its DNS server
        /// </summary>
        public async Task SetServerUrl(ServerUrl value)
        {
            await DeviceStream.SendCommand(new SetServerUrlCommand(DeviceId, value));
        }
    }
}
