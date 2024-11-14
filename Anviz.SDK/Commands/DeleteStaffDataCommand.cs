using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using Anviz.SDK.Utils;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class DeleteStaffDataCommand : Command
    {
        private const byte DELETE_STAFF_DATA = 0x4C;
        private const byte DELETE_STAFF_DATA_ALL = 0x4D;
        private const byte DELETE_FULL = 0xFF;
        public DeleteStaffDataCommand(ulong deviceId, ulong employeeID) : base(deviceId)
        {
            var payload = new byte[6];
            Bytes.Write(5, employeeID).CopyTo(payload, 0);
            payload[5] = DELETE_FULL;
            BuildPayload(DELETE_STAFF_DATA, payload);
        }

        public DeleteStaffDataCommand(ulong deviceId) : base(deviceId)
        {
            var payload = new byte[6];
            payload[5] = DELETE_FULL;
            BuildPayload(DELETE_STAFF_DATA_ALL, payload);
        }

        public DeleteStaffDataCommand(ulong deviceId, ulong employeeID, VerificationMethod method) : base(deviceId)
        {
            var payload = new byte[6];
            Bytes.Write(5, employeeID).CopyTo(payload, 0);
            payload[5] = (byte)method;
            BuildPayload(DELETE_STAFF_DATA, payload);
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        public async Task DeleteEmployeesData(UserInfo user)
        {
            await DeleteEmployeesData(user.Id);
        }

        public async Task DeleteEmployeesData(ulong employeeID)
        {
            await DeviceStream.SendCommand(new DeleteStaffDataCommand(DeviceId, employeeID));
        }

        /// <summary>
        /// Deletes all the employees from the device
        /// </summary>
        /// <returns></returns>
        public async Task DeleteEmployeesData()
        {
            await DeviceStream.SendCommand(new DeleteStaffDataCommand(DeviceId));
        }

        /// <summary>
        /// Delete specific verification data from employee
        /// </summary>
        /// <returns></returns>
        public async Task DeleteEmployeesData(ulong employeeID, VerificationMethod method)
        {
            await DeviceStream.SendCommand(new DeleteStaffDataCommand(DeviceId, employeeID, method));
        }

    }
}