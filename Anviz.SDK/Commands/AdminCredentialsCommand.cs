using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using Anviz.SDK.Utils;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetAdminCredentialsCommand : Command
    {
        private const byte GET_ADMIN_CREDENTIALS = 0x1C;
        public GetAdminCredentialsCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GET_ADMIN_CREDENTIALS, new byte[] { });
        }
    }

    class SetAdminCredentialsCommand : Command
    {
        private const byte SET_ADMIN_CREDENTIALS = 0x1D;
        public SetAdminCredentialsCommand(ulong deviceId, byte[] data) : base(deviceId)
        {
            BuildPayload(SET_ADMIN_CREDENTIALS, data);
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Reads the administrator cards of a T5A
        /// </summary>
        public async Task<AdminCards> GetAdminCards()
        {
            var response = await DeviceStream.SendCommand(new GetAdminCredentialsCommand(DeviceId));
            return new AdminCards(response.DATA);
        }

        /// <summary>
        /// Sets the administrator cards of a T5A
        /// </summary>
        public async Task SetAdminCards(AdminCards value)
        {
            await DeviceStream.SendCommand(new SetAdminCredentialsCommand(DeviceId, value.ToArray()));
        }

        /// <summary>
        /// Reads the administrator password of a T50; null when it has none
        /// </summary>
        public async Task<ulong?> GetAdminPassword()
        {
            var response = await DeviceStream.SendCommand(new GetAdminCredentialsCommand(DeviceId));
            var password = Bytes.Split(response.DATA, 0, 3);
            if (password[0] == 0xFF && password[1] == 0xFF && password[2] == 0xFF)
            {
                return null;
            }
            return Bytes.PasswordRead(password);
        }

        /// <summary>
        /// Sets the administrator password of a T50; null removes it
        /// </summary>
        public async Task SetAdminPassword(ulong? password)
        {
            var data = new byte[13];
            Bytes.PasswordWrite(password).CopyTo(data, 0);
            await DeviceStream.SendCommand(new SetAdminCredentialsCommand(DeviceId, data));
        }
    }
}
