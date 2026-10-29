using Anviz.SDK.Responses;
using System;
using System.Net;

namespace Anviz.SDK.Events
{
    /// <summary>
    /// Who opened the connection
    /// </summary>
    public enum DeviceOrigin
    {
        /// <summary>
        /// The device connected to <see cref="AnvizServer"/> (device in client mode)
        /// </summary>
        Incoming,
        /// <summary>
        /// <see cref="AnvizServer"/> connected to the device (device in server mode)
        /// </summary>
        Outgoing
    }

    public enum DisconnectReason
    {
        /// <summary>
        /// Closed by the device, a network error or a command timeout
        /// </summary>
        Lost,
        /// <summary>
        /// A new connection of a device with the same serial number took its place
        /// </summary>
        Replaced,
        /// <summary>
        /// Nothing was received for <see cref="AnvizServer.HeartbeatTimeout"/>
        /// </summary>
        HeartbeatTimeout,
        /// <summary>
        /// Its address was removed with <see cref="AnvizServer.RemoveDevice"/>
        /// </summary>
        Removed,
        /// <summary>
        /// The server stopped
        /// </summary>
        Stopped,
        /// <summary>
        /// Closed with <see cref="AnvizServer.Disconnect"/>
        /// </summary>
        Closed
    }

    /// <summary>
    /// A connected device, as <see cref="AnvizServer.Devices"/> lists it
    /// </summary>
    public class ConnectedDevice
    {
        public string Serial { get; }
        public AnvizDevice Device { get; }
        public EndPoint EndPoint { get; }
        public DeviceOrigin Origin { get; }
        public DateTime ConnectedAt { get; }
        /// <summary>
        /// The last time something was received from it (ping, record, answer), in UTC
        /// </summary>
        public DateTime LastActivity { get; }

        internal ConnectedDevice(string serial, AnvizDevice device, EndPoint endPoint, DeviceOrigin origin, DateTime connectedAt, DateTime lastActivity)
        {
            Serial = serial;
            Device = device;
            EndPoint = endPoint;
            Origin = origin;
            ConnectedAt = connectedAt;
            LastActivity = lastActivity;
        }
    }

    public class DeviceConnectingEventArgs : EventArgs
    {
        public string Serial { get; }
        public AnvizDevice Device { get; }
        public EndPoint EndPoint { get; }
        public DeviceOrigin Origin { get; }

        internal DeviceConnectingEventArgs(string serial, AnvizDevice device, EndPoint endPoint, DeviceOrigin origin)
        {
            Serial = serial;
            Device = device;
            EndPoint = endPoint;
            Origin = origin;
        }
    }

    public class DeviceConnectedEventArgs : EventArgs
    {
        public string Serial { get; }
        public AnvizDevice Device { get; }
        public EndPoint EndPoint { get; }
        public DeviceOrigin Origin { get; }

        internal DeviceConnectedEventArgs(string serial, AnvizDevice device, EndPoint endPoint, DeviceOrigin origin)
        {
            Serial = serial;
            Device = device;
            EndPoint = endPoint;
            Origin = origin;
        }
    }

    public class DeviceDisconnectedEventArgs : EventArgs
    {
        public string Serial { get; }
        public EndPoint EndPoint { get; }
        public DeviceOrigin Origin { get; }
        public DisconnectReason Reason { get; }

        internal DeviceDisconnectedEventArgs(string serial, EndPoint endPoint, DeviceOrigin origin, DisconnectReason reason)
        {
            Serial = serial;
            EndPoint = endPoint;
            Origin = origin;
            Reason = reason;
        }
    }

    public class DeviceRecordEventArgs : EventArgs
    {
        public string Serial { get; }
        public AnvizDevice Device { get; }
        public Record Record { get; }

        internal DeviceRecordEventArgs(string serial, AnvizDevice device, Record record)
        {
            Serial = serial;
            Device = device;
            Record = record;
        }
    }

    /// <summary>
    /// Something went wrong that did not stop the server: a connection that was not a device or
    /// failed its handshake, an address that could not be reached, an exception of an event handler
    /// </summary>
    public class AnvizServerErrorEventArgs : EventArgs
    {
        /// <summary>
        /// The peer, when the error is about a connection
        /// </summary>
        public EndPoint EndPoint { get; }
        /// <summary>
        /// The serial number, when the device was already known
        /// </summary>
        public string Serial { get; }
        public Exception Exception { get; }

        internal AnvizServerErrorEventArgs(EndPoint endPoint, string serial, Exception exception)
        {
            EndPoint = endPoint;
            Serial = serial;
            Exception = exception;
        }
    }
}
