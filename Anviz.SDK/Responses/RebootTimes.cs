using Anviz.SDK.Utils;
using System;

namespace Anviz.SDK.Responses
{
    /// <summary>
    /// Up to three times of the day the device restarts itself (0x14, 0x15); null is none
    /// </summary>
    public class RebootTimes
    {
        public TimeSpan?[] Times { get; }

        public RebootTimes()
        {
            Times = new TimeSpan?[3];
        }

        internal RebootTimes(byte[] data)
        {
            Times = new TimeSpan?[3];
            for (var i = 0; i < 3; i++)
            {
                var hour = data[i * 3];
                if (hour == 0xFF)
                {
                    continue;
                }
                Times[i] = new TimeSpan(Bytes.ReadBcd(hour), Bytes.ReadBcd(data[i * 3 + 1]),
                    Bytes.ReadBcd(data[i * 3 + 2]));
            }
        }

        internal byte[] ToArray()
        {
            var ret = new byte[9];
            for (var i = 0; i < 3; i++)
            {
                var time = Times[i];
                if (!time.HasValue)
                {
                    ret[i * 3] = 0xFF;
                    ret[i * 3 + 1] = 0xFF;
                    ret[i * 3 + 2] = 0xFF;
                    continue;
                }
                ret[i * 3] = Bytes.WriteBcd(time.Value.Hours);
                ret[i * 3 + 1] = Bytes.WriteBcd(time.Value.Minutes);
                ret[i * 3 + 2] = Bytes.WriteBcd(time.Value.Seconds);
            }
            return ret;
        }
    }
}
