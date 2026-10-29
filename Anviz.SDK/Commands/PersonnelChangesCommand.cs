using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using Anviz.SDK.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Anviz.SDK.Commands
{
    class GetPersonnelChangesCommand : Command
    {
        private const byte GET_PERSONNEL_CHANGES = 0x64;
        public GetPersonnelChangesCommand(ulong deviceId, byte kind) : base(deviceId)
        {
            BuildPayload(GET_PERSONNEL_CHANGES, new byte[] { kind, PersonnelChange.MAX_RECORDS });
        }
    }

    class ClearPersonnelChangesCommand : Command
    {
        private const byte CLEAR_PERSONNEL_CHANGES = 0x1E;
        public ClearPersonnelChangesCommand(ulong deviceId, byte kind, ulong amount) : base(deviceId)
        {
            var data = new byte[4];
            data[0] = kind;
            Bytes.Write(3, amount).CopyTo(data, 1);
            BuildPayload(CLEAR_PERSONNEL_CHANGES, data);
        }
    }
}

namespace Anviz.SDK
{
    public partial class AnvizDevice
    {
        /// <summary>
        /// Reads the changes made to users on the device: all of them, or only the new ones
        /// </summary>
        public async Task<List<PersonnelChange>> GetPersonnelChanges(bool onlyNew)
        {
            var changes = new List<PersonnelChange>();
            byte kind = (byte)(onlyNew ? 2 : 1);
            while (true)
            {
                var response = await DeviceStream.SendCommand(new GetPersonnelChangesCommand(DeviceId, kind));
                var counter = response.DATA[0];
                for (var i = 0; i < counter; i++)
                {
                    changes.Add(new PersonnelChange(response.DATA, 1 + i * PersonnelChange.RECORD_LENGTH));
                }
                if (counter < PersonnelChange.MAX_RECORDS)
                {
                    return changes;
                }
                kind = 0;
            }
        }

        /// <summary>
        /// Deletes every change made to users the device keeps; answers how many
        /// </summary>
        public async Task<ulong> ClearPersonnelChanges()
        {
            var response = await DeviceStream.SendCommand(new ClearPersonnelChangesCommand(DeviceId, 0, 0));
            return Bytes.Read(response.DATA);
        }

        /// <summary>
        /// Marks as read the first changes made to users that were new, or all of them when
        /// amount is null; answers how many
        /// </summary>
        public async Task<ulong> MarkPersonnelChangesRead(ulong? amount = null)
        {
            var kind = (byte)(amount.HasValue ? 2 : 1);
            var response = await DeviceStream.SendCommand(new ClearPersonnelChangesCommand(DeviceId, kind, amount ?? 0));
            return Bytes.Read(response.DATA);
        }
    }
}
