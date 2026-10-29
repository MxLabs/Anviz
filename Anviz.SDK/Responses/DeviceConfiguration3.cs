using Anviz.SDK.Utils;

namespace Anviz.SDK.Responses
{
    /// <summary>
    /// The Wiegand output, whether the device works online, whether a connection must give the
    /// communication password (0x04) and how the door sensor is reported (0x34, 0x35). A value
    /// the device does not use comes, and goes back, as 0xFF.
    /// </summary>
    public class DeviceConfiguration3
    {
        /// <summary>
        /// 0 none, 1 and 2 Wiegand 26 big and little endian, 3 and 4 Wiegand 34 big and little
        /// endian
        /// </summary>
        public byte WiegandMode { get; set; }
        public byte ConnectionMode { get; set; }
        public byte CollectingThreshold { get; set; }
        public byte ConnectionPassword { get; set; }
        public byte DoorSensorReport { get; set; }
        public byte CustomizedTimezone { get; set; }
        public byte AntiPassback { get; set; }
        private readonly byte[] reserved;

        internal DeviceConfiguration3(byte[] data)
        {
            WiegandMode = data[0];
            ConnectionMode = data[1];
            CollectingThreshold = data[2];
            ConnectionPassword = data[3];
            DoorSensorReport = data[4];
            reserved = Bytes.Split(data, 5, 8);
            CustomizedTimezone = data[13];
            AntiPassback = data[14];
        }

        /// <summary>
        /// Whether a new connection must give the communication password (command 0x04)
        /// </summary>
        public bool RequiresConnectionPassword
        {
            get => ConnectionPassword == 1;
            set => ConnectionPassword = (byte)(value ? 1 : 0);
        }

        internal byte[] ToArray()
        {
            var ret = new byte[15];
            ret[0] = WiegandMode;
            ret[1] = ConnectionMode;
            ret[2] = CollectingThreshold;
            ret[3] = ConnectionPassword;
            ret[4] = DoorSensorReport;
            reserved.CopyTo(ret, 5);
            ret[13] = CustomizedTimezone;
            ret[14] = AntiPassback;
            return ret;
        }
    }
}
