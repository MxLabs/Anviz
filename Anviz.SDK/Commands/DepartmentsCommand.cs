using Anviz.SDK.Commands;
using Anviz.SDK.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class AddDepartmentsCommand : Command
    {
        private const byte ADD_DEPARTMENTS = 0x61;
        public AddDepartmentsCommand(ulong deviceId, byte[] data) : base(deviceId)
        {
            BuildPayload(ADD_DEPARTMENTS, data);
        }
    }

    class DeleteDepartmentCommand : Command
    {
        private const byte DELETE_DEPARTMENT = 0x62;
        public DeleteDepartmentCommand(ulong deviceId, ushort id) : base(deviceId)
        {
            BuildPayload(DELETE_DEPARTMENT, Bytes.Write(2, id));
        }
    }
}

namespace Anviz.SDK.Responses
{
    /// <summary>
    /// A department of the device (0x61), under another or none (0)
    /// </summary>
    public class Department
    {
        public ushort Id { get; set; }
        public string Name { get; set; }
        public ushort ParentId { get; set; }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Adds departments to the device, 20 at most each time (OA1000)
        /// </summary>
        public async Task AddDepartments(List<Responses.Department> departments)
        {
            if (departments.Count > 20)
            {
                throw new ArgumentException("A device takes 20 departments at most each time", "departments");
            }
            var data = new byte[1 + departments.Count * 14];
            data[0] = (byte)departments.Count;
            for (var i = 0; i < departments.Count; i++)
            {
                var offset = 1 + i * 14;
                Bytes.Write(2, departments[i].Id).CopyTo(data, offset);
                Bytes.WriteAnsi(10, departments[i].Name).CopyTo(data, offset + 2);
                Bytes.Write(2, departments[i].ParentId).CopyTo(data, offset + 12);
            }
            await DeviceStream.SendCommand(new AddDepartmentsCommand(DeviceId, data));
        }

        /// <summary>
        /// Deletes a department from the device (OA1000)
        /// </summary>
        public async Task DeleteDepartment(ushort id)
        {
            await DeviceStream.SendCommand(new DeleteDepartmentCommand(DeviceId, id));
        }

        /// <summary>
        /// Deletes every department from the device (OA1000)
        /// </summary>
        public async Task DeleteAllDepartments()
        {
            await DeviceStream.SendCommand(new DeleteDepartmentCommand(DeviceId, 0xFFFF));
        }
    }
}
