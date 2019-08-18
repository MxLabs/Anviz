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

        public static byte[] DateTimeToByteArray(DateTime dateTime)
        {
            return new byte[] {
                (byte)(dateTime.Year - 2000),
                (byte)dateTime.Month,
                (byte)dateTime.Day,
                (byte)dateTime.Hour,
                (byte)dateTime.Minute,
                (byte)dateTime.Second
            };
        }

        public static DateTime ByteArrayToDateTime(byte[] d)
        {
            return new DateTime(2000 + d[0], d[1], d[2], d[3], d[4], d[5]);
        }
    }
}
