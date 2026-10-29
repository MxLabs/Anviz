using Anviz.SDK.Commands;
using Anviz.SDK.Utils;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class UserExistsCommand : Command
    {
        private const byte USER_EXISTS = 0x0C;
        public UserExistsCommand(ulong deviceId, ulong userId) : base(deviceId)
        {
            BuildPayload(USER_EXISTS, Bytes.Write(5, userId));
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Says whether the device holds a user
        /// </summary>
        public async Task<bool> UserExists(ulong userId)
        {
            var response = await DeviceStream.SendCommand(new UserExistsCommand(DeviceId, userId));
            return response.DATA[0] == 1;
        }
    }
}
