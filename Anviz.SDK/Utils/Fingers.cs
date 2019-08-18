using System.Collections.Generic;

namespace Anviz.SDK.Utils
{
    public enum Finger
    {
        RightThumb = 0,
        RightIndex = 1,
        RightMiddle = 2,
        RightAnular = 3,
        RightLittle = 4,
        LeftLittle = 5,
        LeftAnular = 6,
        LeftMiddle = 7,
        LeftIndex = 8,
        LeftThumb = 9,
    }

    public static class Fingers
    {
        public static List<Finger> DecodeFingers(ulong value)
        {
            var ret = new List<Finger>();
            for (var i = 0; i < 10; i++)
            {
                if ((value & (ulong)(1 << i)) != 0)
                {
                    ret.Add((Finger)i);
                }
            }
            return ret;
        }

        public static ulong EncodeFingers(IEnumerable<Finger> value)
        {
            int ret = 0;
            foreach (var f in value)
            {
                ret |= 1 << (int)f;
            }
            return (ulong)ret;
        }
    }
}
