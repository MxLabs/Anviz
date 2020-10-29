namespace Anviz.SDK.Events
{
    public class DeviceLostEventArgs
    {
        public string Serial { get; }
        public DeviceLostEventArgs(string serial)
        {
            Serial = serial;
        }
    }
}
