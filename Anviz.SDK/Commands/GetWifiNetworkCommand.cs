using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetWifiNetworkCommand : Command
    {
        private const byte GETWIFINETWORK = 0x6E;
        public GetWifiNetworkCommand(ulong deviceId, byte number) : base(deviceId)
        {
            BuildPayload(GETWIFINETWORK, new byte[] { number });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Reads one of the five WiFi networks the device knows, 1 to 5
        /// </summary>
        public async Task<WifiNetwork> GetWifiNetwork(byte number)
        {
            if (number < 1 || number > WifiNetwork.MAX_NETWORKS)
            {
                throw new ArgumentOutOfRangeException("number", "Must be within 1 to WifiNetwork.MAX_NETWORKS");
            }
            var response = await DeviceStream.SendCommand(new GetWifiNetworkCommand(DeviceId, number));
            return new WifiNetwork(number, response.DATA);
        }
    }
}
