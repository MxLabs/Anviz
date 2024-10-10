using Anviz.SDK.Utils;
using System;

namespace Anviz.SDK.Responses
{
    [Flags]
    public enum DateFormat
    {
        YYMMDD = 8,
        MMDDYY = 16,
        DDMMYY = 32,
    }

    public enum Volume
    {
        Off,
        Lowest,
        Low,
        Medium,
        High,
        Maximum
    }

    public enum Language : byte
    {
        SimplifiedChinese,
        TraditionalChinese,
        English,
        French,
        Spanish,
        Portuguese
    }

    public class BasicSettings
    {
        public string Firmware { get; }
        public ulong ManagementPassword { get; set; }
        public byte Sleep { get; set; }
        public Volume Volume { get; set; }
        public Language Language { get; set; }
        public DateFormat DateFormat { get; set; }
        public bool Is24HourClock { get; set; }
        public byte Attendance { get; set; }
        public byte LangCHG { get; set; }
        public byte CMDVersion { get; }

        internal BasicSettings(byte[] data)
        {
            Firmware = Bytes.GetAsciiString(Bytes.Split(data, 0, 8));
            ManagementPassword = Bytes.PasswordRead(Bytes.Split(data, 8, 3));
            Sleep = data[11];
            Volume = (Volume)data[12];
            Language = (Language)data[13];
            DateFormat = (DateFormat)(data[14] & 0xFE);
            Is24HourClock = (data[14] & 0x01) == 0;
            Attendance = data[15];
            LangCHG = data[16];
            CMDVersion = data[17];
        }

        internal byte[] ToArray()
        {
            var ret = new byte[10];
            Bytes.PasswordWrite(ManagementPassword).CopyTo(ret, 0);
            ret[3] = Sleep;
            ret[4] = (byte)Volume;
            ret[5] = (byte)Language;
            ret[6] = (byte)(DateFormat + (Is24HourClock ? 0 : 1));
            ret[7] = Attendance;
            ret[8] = LangCHG;
            ret[9] = 0; //reserved
            return ret;
        }
    }
}
