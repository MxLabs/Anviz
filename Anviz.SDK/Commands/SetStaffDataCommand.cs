using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using Anviz.SDK.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class SetStaffDataCommand : Command
    {
        private const byte SET_STAFF_DATA = 0x73;
        public SetStaffDataCommand(ulong deviceId, List<UserInfo> users) : base(deviceId)
        {
            var userAmount = (byte)Math.Min(users.Count, UserInfo.MAX_RECORDS);
            var payload = new byte[1 + userAmount * UserInfo.RECORD_LENGTH];
            payload[0] = userAmount;
            for (var i = 0; i < userAmount; i++)
            {
                users[i].ToArray().CopyTo(payload, 1 + i * UserInfo.RECORD_LENGTH);
            }
            users.RemoveRange(0, userAmount);
            BuildPayload(SET_STAFF_DATA, payload);
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task SetEmployeesData(UserInfo user)
        {
            await SetEmployeesData(new List<UserInfo> { user });
        }

        /// <summary>
        /// Writes the employees, 8 per command. The answer has a bit per employee sent, set when the
        /// device stored it: an employee it did not store throws <see cref="AnvizDeviceException"/>
        /// with <see cref="ResponseStatus.Fail"/> (an M7 clears it, found by level54; a W1 and a W2PRO
        /// answer all set and refuse with an error instead)
        /// </summary>
        public async Task SetEmployeesData(List<UserInfo> users)
        {
            // the command takes the users it sends out of the list: work on a copy of the caller's
            users = new List<UserInfo>(users);
            while (users.Count > 0)
            {
                var sent = Math.Min(users.Count, UserInfo.MAX_RECORDS);
                var response = await DeviceStream.SendCommand(new SetStaffDataCommand(DeviceId, users));
                var stored = (1UL << sent) - 1;
                // a device that answers no flag says nothing more than its RET
                if (response.DATA.Length > 0 && (Bytes.Read(response.DATA) & stored) != stored)
                {
                    throw new AnvizDeviceException(ResponseStatus.Fail);
                }
            }
        }
    }
}