using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetStaffDataCommand : Command
    {
        private const byte GET_STAFF_DATA = 0x72;
        public GetStaffDataCommand(ulong deviceId, bool isFirst, ulong amount) : base(deviceId)
        {
            var kind = (byte)(isFirst ? 1 : 0);
            BuildPayload(GET_STAFF_DATA, new byte[] { kind, (byte)Math.Min(amount, UserInfo.MAX_RECORDS) });
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task<List<UserInfo>> GetEmployeesData()
        {
            var statistics = await GetDownloadInformation();
            var userAmount = statistics.UserAmount;
            var users = new List<UserInfo>((int)userAmount);
            var isFirst = true;
            while (userAmount > 0)
            {
                var response = await DeviceStream.SendCommand(new GetStaffDataCommand(DeviceId, isFirst, userAmount));
                var counter = response.DATA[0];
                userAmount -= counter;
                for (var i = 0; i < counter; i++)
                {
                    users.Add(new UserInfo(response.DATA, 1 + i * UserInfo.RECORD_LENGTH));
                }
                isFirst = false;
            }
            return users;
        }
    }
}