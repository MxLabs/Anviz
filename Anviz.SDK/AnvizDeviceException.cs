using Anviz.SDK.Responses;
using System;

namespace Anviz.SDK
{
    /// <summary>
    /// The device answered a command with a RET other than success. The connection stays open
    /// </summary>
    public class AnvizDeviceException : Exception
    {
        public ResponseStatus Status { get; }

        public AnvizDeviceException(ResponseStatus status) : base("RET: " + status)
        {
            Status = status;
        }
    }
}
