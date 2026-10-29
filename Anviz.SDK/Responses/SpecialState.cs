namespace Anviz.SDK.Responses
{
    /// <summary>
    /// The state of the door a device controls (0x2F), with the bits of the CrossChex SDK. On a
    /// W2PRO LockOpen is always set and DoorOpen is set while UnlockDoor keeps the lock released;
    /// a W1 with the relay on the bell answers 0
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
