namespace Anviz.SDK.Responses
{
    /// <summary>
    /// The state of the door a device controls (0x2F)
    /// </summary>
    public class SpecialState
    {
        public byte Raw { get; }
        public bool DoorAlarm => (Raw & 0x02) != 0;
        public bool DoorOpen => (Raw & 0x20) != 0;
        public bool DoorSensorOpen => (Raw & 0x40) != 0;
        public bool LockOpen => (Raw & 0x80) != 0;

        internal SpecialState(byte[] data)
        {
            Raw = data[0];
        }
    }
}
