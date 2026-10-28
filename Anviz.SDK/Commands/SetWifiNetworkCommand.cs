using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetWifiNetworkCommand : Command
    {
        private const byte SETWIFINETWORK = 0x6F;
        public SetWifiNetworkCommand(ulong deviceId, WifiNetwork value) : base(deviceId)
        {
            BuildPayload(SETWIFINETWORK, value.ToArray());
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Sets one of the five WiFi networks the device knows
        /// </summary>
        public async Task SetWifiNetwork(WifiNetwork value)
        {
            await DeviceStream.SendCommand(new SetWifiNetworkCommand(DeviceId, value));
        }
    }
}
