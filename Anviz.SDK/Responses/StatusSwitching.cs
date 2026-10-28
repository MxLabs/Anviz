using System.Collections.Generic;

namespace Anviz.SDK.Responses
{
    /// <summary>
    /// When the device switches by itself to an attendance status (0x20, 0x21): a stretch of time
    /// on each day of the week, Monday first. The hours and minutes are kept as the device gives
    /// them, so a setting read is written back the same.
    /// </summary>
    public class StatusSwitching
    {
        public class Stretch
        {
            public byte FromHour { get; set; }
            public byte FromMinute { get; set; }
            public byte ToHour { get; set; }
            public byte ToMinute { get; set; }
        }

        public byte Number { get; }
        public List<Stretch> Days { get; }
        public byte Status { get; set; }

        public StatusSwitching(byte number)
        {
            Number = number;
            Days = new List<Stretch>();
            for (var i = 0; i < 7; i++)
            {
                Days.Add(new Stretch());
            }
        }

        internal StatusSwitching(byte number, byte[] data)
        {
            Number = number;
            Days = new List<Stretch>();
            for (var i = 0; i < 7; i++)
            {
                Days.Add(new Stretch
                {
                    FromHour = data[i * 4],
                    FromMinute = data[i * 4 + 1],
                    ToHour = data[i * 4 + 2],
                    ToMinute = data[i * 4 + 3]
                });
            }
            Status = data[28];
        }

        internal byte[] ToArray()
        {
            var ret = new byte[30];
            ret[0] = Number;
            for (var i = 0; i < 7; i++)
            {
                ret[1 + i * 4] = Days[i].FromHour;
                ret[2 + i * 4] = Days[i].FromMinute;
                ret[3 + i * 4] = Days[i].ToHour;
                ret[4 + i * 4] = Days[i].ToMinute;
            }
            ret[29] = Status;
            return ret;
        }
    }
}
