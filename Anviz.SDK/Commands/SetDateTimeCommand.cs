using Anviz.SDK.Commands;
using Anviz.SDK.Utils;
using System;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetDateTimeCommand : Command
    {
        private const byte SET_DATETIME = 0x39;
        public SetDateTimeCommand(ulong deviceId, DateTime dateTime) : base(deviceId)
        {
            BuildPayload(SET_DATETIME, DateConversions.DateTimeToByteArray(dateTime));
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task SetDateTime(DateTime dateTime)
        {
            var cmd = new SetDateTimeCommand(DeviceId, dateTime);
            await DeviceStream.SendCommand(cmd);
        }
    }
}