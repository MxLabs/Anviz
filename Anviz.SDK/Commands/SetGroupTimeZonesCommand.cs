using Anviz.SDK.Commands;
using System;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetGroupTimeZonesCommand : Command
    {
        private const byte SETGROUPTIMEZONES = 0x53;
        public SetGroupTimeZonesCommand(ulong deviceId, byte group, byte[] timeZones) : base(deviceId)
        {
            BuildPayload(SETGROUPTIMEZONES, new byte[] { group, timeZones[0], timeZones[1], timeZones[2], timeZones[3] });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Sets the four time zones (0x51) of an access group, 2 to 16
        /// </summary>
        public async Task SetGroupTimeZones(byte group, byte[] timeZones)
        {
            if (timeZones.Length != 4)
            {
                throw new ArgumentException("A group has four time zones", "timeZones");
            }
            await DeviceStream.SendCommand(new SetGroupTimeZonesCommand(DeviceId, group, timeZones));
        }
    }
}
