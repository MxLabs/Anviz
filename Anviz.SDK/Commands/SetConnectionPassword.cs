using Anviz.SDK.Commands;
using System.Text;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetConnectionPassword : Command
    {
        private const byte SET_CONNECTION_PASSWORD = 0x04;
        public SetConnectionPassword(ulong deviceId, string user, string password) : base(deviceId)
        {
            var payload = new byte[24];
            Encoding.ASCII.GetBytes(user).CopyTo(payload, 0);
            Encoding.ASCII.GetBytes(password).CopyTo(payload, 12);
            BuildPayload(SET_CONNECTION_PASSWORD, payload);
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task SetConnectionPassword(string user, string password)
        {
            var cmd = new SetConnectionPassword(DeviceId, user, password);
            await DeviceStream.SendCommand(cmd);
        }
    }
}