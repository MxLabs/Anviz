using System;
using System.Collections.Generic;

namespace Anviz.SDK.Responses
{
    /// <summary>
    /// When the device switches by itself to an attendance status (0x20, 0x21): a stretch of time
    /// on each day of the week, Monday first
    /// </summary>
    public class StatusSwitching
    {
        public class Stretch
        {
            public TimeSpan From { get; set; }
            public TimeSpan To { get; set; }
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
                    From = new TimeSpan(data[i * 4], data[i * 4 + 1], 0),
                    To = new TimeSpan(data[i * 4 + 2], data[i * 4 + 3], 0)
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
                ret[1 + i * 4] = (byte)Days[i].From.Hours;
                ret[2 + i * 4] = (byte)Days[i].From.Minutes;
                ret[3 + i * 4] = (byte)Days[i].To.Hours;
                ret[4 + i * 4] = (byte)Days[i].To.Minutes;
            }
            ret[29] = Status;
            return ret;
        }
    }
}
