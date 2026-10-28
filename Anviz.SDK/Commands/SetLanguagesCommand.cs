using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetLanguagesCommand : Command
    {
        private const byte SETLANGUAGES = 0x19;
        public SetLanguagesCommand(ulong deviceId, byte[] languages) : base(deviceId)
        {
            BuildPayload(SETLANGUAGES, languages);
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Sets up to four languages the device lets choose among
        /// </summary>
        public async Task SetLanguages(DeviceLanguage[] languages)
        {
            if (languages.Length > 4)
            {
                throw new ArgumentException("A device keeps four languages at most", "languages");
            }
            var data = new byte[4];
            for (var i = 0; i < 4; i++)
            {
                data[i] = i < languages.Length ? (byte)languages[i] : (byte)DeviceLanguage.None;
            }
            await DeviceStream.SendCommand(new SetLanguagesCommand(DeviceId, data));
        }
    }
}
