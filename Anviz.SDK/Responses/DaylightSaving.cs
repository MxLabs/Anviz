namespace Anviz.SDK.Responses
{
    /// <summary>
    /// When the device moves its clock for daylight saving time (0x1A, 0x1B): on a date, or on a
    /// day of a week of a month
    /// </summary>
    public class DaylightSaving
    {
        /// <summary>
        /// A moment of the change: a date (Month, Day) or, by week, the week of the month and the
        /// day of the week; and the time. The protocol gives the week of the month as 0x01 to
        /// 0x04 ("previous 1-4 weeks") or 0x81 to 0x82 ("upcoming 1-2 weeks"), and the day of
        /// the week as 0 Sunday to 6 Saturday
        /// </summary>
        public class Moment
        {
            public byte Month { get; set; }
            public byte Day { get; set; }
            public byte WeekOfMonth { get; set; }
            public byte DayOfWeek { get; set; }
            public byte Hour { get; set; }
            public byte Minute { get; set; }
            public byte Second { get; set; }

            public Moment()
            {
            }

            internal Moment(byte[] data, int offset)
            {
                Month = data[offset];
                Day = data[offset + 1];
                WeekOfMonth = data[offset + 2];
                DayOfWeek = data[offset + 3];
                Hour = data[offset + 4];
                Minute = data[offset + 5];
                Second = data[offset + 6];
            }

            internal void CopyTo(byte[] ret, int offset)
            {
                ret[offset] = Month;
                ret[offset + 1] = Day;
                ret[offset + 2] = WeekOfMonth;
                ret[offset + 3] = DayOfWeek;
                ret[offset + 4] = Hour;
                ret[offset + 5] = Minute;
                ret[offset + 6] = Second;
            }
        }

        public bool Enabled { get; set; }
        public bool ByWeek { get; set; }
        public Moment Start { get; set; }
        public Moment End { get; set; }

        public DaylightSaving()
        {
            Start = new Moment();
            End = new Moment();
        }

        internal DaylightSaving(byte[] data)
        {
            Enabled = data[0] == 1;
            ByWeek = data[1] == 2;
            Start = new Moment(data, 2);
            End = new Moment(data, 9);
        }

        internal byte[] ToArray()
        {
            var ret = new byte[16];
            ret[0] = (byte)(Enabled ? 1 : 0);
            ret[1] = (byte)(ByWeek ? 2 : 1);
            Start.CopyTo(ret, 2);
            End.CopyTo(ret, 9);
            return ret;
        }
    }
}
