using Anviz.SDK.Utils;
using System;

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