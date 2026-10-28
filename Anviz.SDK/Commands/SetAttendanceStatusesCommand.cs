using Anviz.SDK.Commands;
using System;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetAttendanceStatusesCommand : Command
    {
        private const byte SETATTENDANCESTATUSES = 0x5B;
        public SetAttendanceStatusesCommand(ulong deviceId, byte[] statuses) : base(deviceId)
        {
            BuildPayload(SETATTENDANCESTATUSES, statuses);
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Sets the attendance statuses the device offers, up to 16 indexes: the rest are 0xFF.
        /// The device then shows these instead of its customized ones (0x71).
        /// </summary>
        public async Task SetAttendanceStatuses(byte[] statuses)
        {
            if (statuses.Length > 16)
            {
                throw new ArgumentException("A device offers 16 statuses at most", "statuses");
            }
            var data = new byte[16];
            for (var i = 0; i < 16; i++)
            {
                data[i] = i < statuses.Length ? statuses[i] : (byte)0xFF;
            }
            await DeviceStream.SendCommand(new SetAttendanceStatusesCommand(DeviceId, data));
        }
    }
}
