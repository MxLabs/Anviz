using System.Text;

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
