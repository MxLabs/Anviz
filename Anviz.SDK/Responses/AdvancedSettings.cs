using Anviz.SDK.Utils;

namespace Anviz.SDK.Responses
{
    public enum FPPrecision
    {
        Low,
        Medium,
        High
    }

    public class AdvancedSettings
    {
        public FPPrecision FPPrecision { get; set; }
        public byte WiegandHead { get; set; }
        public byte WiegandMode { get; set; }
        public bool WorkCode { get; set; }
        public bool RealTimeMode { get; set; }
        public bool FPAutoUpdate { get; set; }
        public byte RelayMode { get; set; }
        public byte LockDelay { get; set; }
        public ulong MemoryFullAlarm { get; set; }
        public byte RepeatAttendanceDelay { get; set; }
        public byte DoorSensorDelay { get; set; }
        public byte ScheduledBellDelay { get; set; }
        public byte Reserved { get; set; }

        internal AdvancedSettings(byte[] data)
        {
            FPPrecision = (FPPrecision)data[0];
            WiegandHead = data[1];
            WiegandMode = data[2];
            WorkCode = data[3] > 0;
            RealTimeMode = data[4] > 0;
            FPAutoUpdate = data[5] > 0;
            RelayMode = data[6];
            LockDelay = data[7];
            MemoryFullAlarm = Bytes.Read(Bytes.Split(data, 8, 3));
            RepeatAttendanceDelay = data[11];
            DoorSensorDelay = data[12];
            ScheduledBellDelay = data[13];
            Reserved = data[14];
        }
    }
}
