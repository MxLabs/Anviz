using Anviz.SDK.Utils;

namespace Anviz.SDK.Responses
{
    public class BasicSettings
    {
        public string Firmware { get; set; }
        public byte[] PWD { get; set; }
        public byte Sleep { get; set; }
        public byte Volume { get; set; }
        public byte Language { get; set; }
        public byte DateTimeFormat { get; set; }
        public byte Attendance { get; set; }
        public byte LangCHG { get; set; }
        public byte CMDVersion { get; set; }

        internal BasicSettings(byte[] data)
        {
            Firmware = Bytes.GetAsciiString(Bytes.Split(data, 0, 8));
            PWD = Bytes.Split(data, 8, 3);
            Sleep = data[11];
            Volume = data[12];
            Language = data[13];
            DateTimeFormat = data[14];
            Attendance = data[15];
            LangCHG = data[16];
            CMDVersion = data[17];
        }
    }
}
