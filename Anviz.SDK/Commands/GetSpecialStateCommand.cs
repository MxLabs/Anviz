using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetSpecialStateCommand : Command
    {
        private const byte GET_SPECIALSTATE = 0x2F;
        public GetSpecialStateCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GET_SPECIALSTATE, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Reads the state of the door: its alarm, the door, its sensor and its lock
        /// </summary>
        public async Task<SpecialState> GetSpecialState()
        {
            var response = await DeviceStream.SendCommand(new GetSpecialStateCommand(DeviceId));
            return new SpecialState(response.DATA);
        }
    }
}
