using System;

namespace Anviz.SDK.Events
{
    public class DeviceConnectedEventArgs : EventArgs
    {
        public string Serial { get; }
        public AnvizDevice Device { get; }
        internal DeviceConnectedEventArgs(string serial, AnvizDevice device)
        {
            Serial = serial;
            Device = device;
        }
    }
}
