using Anviz.SDK.Commands;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetGroupTimeZonesCommand : Command
    {
        private const byte GETGROUPTIMEZONES = 0x52;
        public GetGroupTimeZonesCommand(ulong deviceId, byte group) : base(deviceId)
        {
            BuildPayload(GETGROUPTIMEZONES, new byte[] { group });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Reads the four time zones (0x50) of an access group, 2 to 16: groups 0 and 1 are
        /// always closed and always open
        /// </summary>
        public async Task<byte[]> GetGroupTimeZones(byte group)
        {
            var response = await DeviceStream.SendCommand(new GetGroupTimeZonesCommand(DeviceId, group));
            return response.DATA;
        }
    }
}
