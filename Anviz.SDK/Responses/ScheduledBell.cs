using System;

namespace Anviz.SDK.Responses
{
    public class ScheduledBell
    {
        internal const int RECORD_LENGTH = 3;
        public const int MAX_SCHEDULED_BELL_SLOT = 30;

        public TimeSpan Time { get; set; }

        [Flags]
        public enum DayName
        {
            Sunday = 1 << 0,
            Monday = 1 << 1,
            Tuesday = 1 << 2,
            Wednesday = 1 << 3,
            Thursday = 1 << 4,
            Friday = 1 << 5,
            Saturday = 1 << 6,
        }

        public DayName Days { get; set; }

        public bool HasValue => Days != 0 && Days != (DayName)255;

        public ScheduledBell()
        {
            Days = 0;
            Time = new TimeSpan(0, 0, 0);
        }

        internal ScheduledBell(byte[] data, int position)
        {
            Time = new TimeSpan(data[position + 0], data[position + 1], 0);
            Days = (DayName)data[position + 2];
        }

        internal byte[] ToArray(byte number)
        {
            return new byte[] { number, (byte)Time.Hours, (byte)Time.Minutes, (byte)Days };
        }
    }
}
