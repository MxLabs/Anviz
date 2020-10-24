using Anviz.SDK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Anviz.SDK.Responses
{
    public class AnvizTimeZone
    {
        public const int MAX_TIMEZONE_SLOT = 30;

        public enum DayName
        {
            Monday,
            Tuesday,
            Wednesday,
            Thursday,
            Friday,
            Saturday,
            Sunday
        }

        public class Day
        {
            public TimeSpan From { get; set; }
            public TimeSpan To { get; set; }

            public Day()
            {

            }

            internal Day(byte[] data)
            {
                From = new TimeSpan(data[0], data[1], 0);
                To = new TimeSpan(data[2], data[3], 0);
            }

            internal byte[] ToArray()
            {
                return new byte[] {
                    (byte)From.Hours,
                    (byte)From.Minutes,
                    (byte)To.Hours,
                    (byte)To.Minutes
                };
            }
        }

        public Dictionary<DayName, Day> Days { get; }

        internal AnvizTimeZone(byte[] data)
        {
            Days = new Dictionary<DayName, Day>();
            for (var day = 0; day < 7; day++)
            {
                Days.Add((DayName)day, new Day(Bytes.Split(data, day * 4, 4)));
            }
        }

        internal byte[] ToArray(byte number)
        {
            var ret = new List<byte[]>
            {
                new byte[] { number }
            };
            for (var day = 0; day < 6; day++)
            {
                ret.Add(Days[(DayName)day].ToArray());
            }
            return ret.SelectMany(x => x).ToArray();
        }
    }
}
