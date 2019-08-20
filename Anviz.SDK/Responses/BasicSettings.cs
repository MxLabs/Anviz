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

    public class BasicSettings
    {
        public string Firmware { get; set; }
        public ulong ManagementPassword { get; set; }
        public byte Sleep { get; set; }
        public byte Volume { get; set; }
        public byte Language { get; set; }
        public DateFormat DateFormat { get; set; }
        public bool Is24HourClock { get; set; }
        public byte Attendance { get; set; }
        public byte LangCHG { get; set; }
        public byte CMDVersion { get; set; }

        internal BasicSettings(byte[] data)
        {
            Firmware = Bytes.GetAsciiString(Bytes.Split(data, 0, 8));
            ManagementPassword = Bytes.PasswordRead(Bytes.Split(data, 8, 3));
            Sleep = data[11];
            Volume = data[12];
            Language = data[13];
            DateFormat = (DateFormat)(data[14] & 0xFE);
            Is24HourClock = (data[14] & 0x01) == 0;
            Attendance = data[15];
            LangCHG = data[16];
            CMDVersion = data[17];
        }
    }
}
