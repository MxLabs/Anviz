using System;

namespace Anviz.SDK.Utils
{
    public static class DateConversions
    {
        public static readonly DateTime RecordEpoch = new DateTime(2000, 1, 2);
        public static DateTime RecordToDateTime(ulong value)
        {
            return RecordEpoch.AddSeconds(value);
        }
    }
}
