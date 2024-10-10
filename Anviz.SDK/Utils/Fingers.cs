using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Anviz.SDK.Utils
{
    [Flags]
    public enum VerificationMethod : byte
    {
        [Description("None")]
        None = 0x0,
        [Description("Right Thumb")]
        RightThumb = 0x1,
        [Description("Right Index")]
        RightIndex = 0x2,
        [Description("Password")]
        Password = 0x4,
        [Description("Card")]
        RFCard = 0x8,
        [Description("Right Middle")]
        RightMiddle = 0x10,
        [Description("Right Anular")]
        RightAnular = 0x20,
        [Description("Right Little")]
        RightLittle = 0x30,
        [Description("Left Little")]
        LeftLittle = 0x40,
        [Description("Left Anular")]
        LeftAnular = 0x50,
        [Description("Left Middle")]
        LeftMiddle = 0x60,
        [Description("Left Index")]
        LeftIndex = 0x70,
        [Description("Left Thumb")]
        LeftThumb = 0x80,
        [Description("All")]
        All = 0xFF
    }

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
