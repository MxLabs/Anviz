using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using Anviz.SDK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetMessageCommand : Command
    {
        private const byte GET_MESSAGE = 0x56;
        public GetMessageCommand(ulong deviceId, byte index) : base(deviceId)
        {
            BuildPayload(GET_MESSAGE, new byte[] { index });
        }
    }

    class AddMessageCommand : Command
    {
        private const byte ADD_MESSAGE = 0x57;
        public AddMessageCommand(ulong deviceId, DeviceMessage message) : base(deviceId)
        {
            BuildPayload(ADD_MESSAGE, message.ToArray());
        }
    }

    class GetMessageHeadersCommand : Command
    {
        private const byte GET_MESSAGE_HEADERS = 0x58;
        public GetMessageHeadersCommand(ulong deviceId) : base(deviceId)
        {
            BuildPayload(GET_MESSAGE_HEADERS, new byte[] { });
        }
    }

    class DeleteMessageCommand : Command
    {
        private const byte DELETE_MESSAGE = 0x59;
        public DeleteMessageCommand(ulong deviceId, byte index) : base(deviceId)
        {
            BuildPayload(DELETE_MESSAGE, new byte[] { index });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Reads the message at an index, 0 to 49; null when there is none
        /// </summary>
        public async Task<DeviceMessage> GetMessage(byte index)
        {
            CheckMessageIndex(index);
            var response = await DeviceStream.SendCommand(new GetMessageCommand(DeviceId, index));
            if (DeviceMessage.IsEmpty(response.DATA))
            {
                return null;
            }
            return new DeviceMessage(response.DATA);
        }

        /// <summary>
        /// Reads whose each message is and when the device shows it, of the messages it holds
        /// </summary>
        public async Task<List<DeviceMessageHeader>> GetMessageHeaders()
        {
            var response = await DeviceStream.SendCommand(new GetMessageHeadersCommand(DeviceId));
            var headers = new List<DeviceMessageHeader>();
            for (var i = 0; i < DeviceMessage.MAX_MESSAGES; i++)
            {
                var header = Bytes.Split(response.DATA, i * 11, 11);
                if (header.All(b => b == 0xFF))
                {
                    continue;
                }
                headers.Add(new DeviceMessageHeader((byte)i, header));
            }
            return headers;
        }

        /// <summary>
        /// Adds a message; the device keeps it at the first free index, in the encoding the
        /// message says (UTF-16 unless told otherwise)
        /// </summary>
        public async Task AddMessage(DeviceMessage message)
        {
            await DeviceStream.SendCommand(new AddMessageCommand(DeviceId, message));
        }

        /// <summary>
        /// Deletes the message at an index, 0 to 49
        /// </summary>
        public async Task DeleteMessage(byte index)
        {
            CheckMessageIndex(index);
            await DeviceStream.SendCommand(new DeleteMessageCommand(DeviceId, index));
        }

        private static void CheckMessageIndex(byte index)
        {
            if (index >= DeviceMessage.MAX_MESSAGES)
            {
                throw new ArgumentOutOfRangeException("index", "Must be within 0 to DeviceMessage.MAX_MESSAGES - 1");
            }
        }

        /// <summary>
        /// Deletes every message the device holds
        /// </summary>
        public async Task DeleteAllMessages()
        {
            await DeviceStream.SendCommand(new DeleteMessageCommand(DeviceId, 0xFF));
        }
    }
}
