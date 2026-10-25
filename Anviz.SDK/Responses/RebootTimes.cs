using Anviz.SDK.Utils;
using System;

namespace Anviz.SDK.Responses
{
    /// <summary>
    /// Up to three times of the day the device restarts itself (0x14, 0x15), kept as the BCD
    /// bytes the device gives, so a setting read is written back the same
    /// </summary>
    public class RebootTimes
    {
        public class RebootTime
        {
            public byte Hour { get; set; } = 0xFF;
            public byte Minute { get; set; } = 0xFF;
            public byte Second { get; set; } = 0xFF;

            /// <summary>
            /// The time it says; null when it is not set (0xFF)
            /// </summary>
            public TimeSpan? Time
            {
                get
                {
                    if (Hour == 0xFF)
                    {
                        return null;
                    }
                    return new TimeSpan(Bytes.ReadBcd(Hour), Bytes.ReadBcd(Minute), Bytes.ReadBcd(Second));
                }
                set
                {
                    if (!value.HasValue)
                    {
                        Hour = 0xFF;
                        Minute = 0xFF;
                        Second = 0xFF;
                        return;
                    }
                    Hour = Bytes.WriteBcd(value.Value.Hours);
                    Minute = Bytes.WriteBcd(value.Value.Minutes);
                    Second = Bytes.WriteBcd(value.Value.Seconds);
                }
            }
        }

        public RebootTime[] Times { get; }

        public RebootTimes()
        {
            Times = new[] { new RebootTime(), new RebootTime(), new RebootTime() };
        }

        internal RebootTimes(byte[] data)
        {
            Times = new RebootTime[3];
            for (var i = 0; i < 3; i++)
            {
                Times[i] = new RebootTime
                {
                    Hour = data[i * 3],
                    Minute = data[i * 3 + 1],
                    Second = data[i * 3 + 2]
                };
            }
        }

        internal byte[] ToArray()
        {
            var ret = new byte[9];
            for (var i = 0; i < 3; i++)
            {
                ret[i * 3] = Times[i].Hour;
                ret[i * 3 + 1] = Times[i].Minute;
                ret[i * 3 + 2] = Times[i].Second;
            }
            return ret;
        }
    }
}
