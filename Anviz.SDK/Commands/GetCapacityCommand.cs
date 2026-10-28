using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetCapacityCommand : Command
    {
        private const byte GET_CAPACITY = 0x5D;
        public GetCapacityCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GET_CAPACITY, new byte[] { });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Reads how many users, fingerprints and records the device holds at most
        /// </summary>
        public async Task<DeviceCapacity> GetCapacity()
        {
            var response = await DeviceStream.SendCommand(new GetCapacityCommand(DeviceId));
            return new DeviceCapacity(response.DATA);
        }
    }
}
