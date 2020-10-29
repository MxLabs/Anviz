using Anviz.SDK.Utils;

namespace Anviz.SDK.Responses
{
    public enum FPPrecision
    {
        Low,
        Medium,
        High
    }

    public enum RelayMode
    {
        Door = 0,
        Bell = 1,
        None = 3
    }

    public class AdvancedSettings
    {
        public FPPrecision FPPrecision { get; set; }
        public byte WiegandHead { get; set; }
        public byte WiegandMode { get; set; }
        public bool WorkCode { get; set; }
        public bool RealTimeMode { get; set; }
        public bool FPAutoUpdate { get; set; }
        public RelayMode RelayMode { get; set; }
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
            RelayMode = (RelayMode)data[6];
            LockDelay = data[7];
            MemoryFullAlarm = Bytes.Read(Bytes.Split(data, 8, 3));
            RepeatAttendanceDelay = data[11];
            DoorSensorDelay = data[12];
            ScheduledBellDelay = data[13];
            Reserved = data[14];
        }

        internal byte[] ToArray()
        {
            var ret = new byte[15];
            ret[0] = (byte)FPPrecision;
            ret[1] = WiegandHead;
            ret[2] = WiegandMode;
            ret[3] = (byte)(WorkCode ? 1 : 0);
            ret[4] = (byte)(RealTimeMode ? 1 : 0);
            ret[5] = (byte)(FPAutoUpdate ? 1 : 0);
            ret[6] = (byte)RelayMode;
            ret[7] = LockDelay;
            Bytes.Write(3, MemoryFullAlarm).CopyTo(ret, 8);
            ret[11] = RepeatAttendanceDelay;
            ret[12] = DoorSensorDelay;
            ret[13] = ScheduledBellDelay;
            ret[14] = Reserved;
            return ret;
        }
    }
}
