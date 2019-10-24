using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using Anviz.SDK.Utils;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Anviz.SDK
{
    public class AnvizDevice : IDisposable
    {
        private const int DEVICE_TIMEOUT = 10000; //10 seconds
        public ulong DeviceId { get; private set; } = 0;

        private readonly TcpClient DeviceSocket;
        private readonly NetworkStream DeviceStream;

        public AnvizDevice(TcpClient socket)
        {
            DeviceSocket = socket;
            DeviceStream = socket.GetStream();
            DeviceSocket.ReceiveTimeout = DEVICE_TIMEOUT;
            DeviceSocket.SendTimeout = DEVICE_TIMEOUT;
            DeviceStream.WriteTimeout = DEVICE_TIMEOUT;
            DeviceStream.ReadTimeout = DEVICE_TIMEOUT;
        }

        private async Task<Response> SendCommand(Command cmd)
        {
            await cmd.Send(DeviceStream);
            return await Response.FromStream(cmd.ResponseCode, DeviceStream);
        }

        public async Task SetConnectionPassword(string user, string password)
        {
            var cmd = new SetConnectionPassword(DeviceId,user,password);
            await SendCommand(cmd);
        }

        public async Task<DateTime> GetDateTime()
        {
            var cmd = new GetDateTimeCommand(DeviceId);
            var response = await SendCommand(cmd);
            return DateConversions.ByteArrayToDateTime(response.DATA);
        }

        public async Task SetDateTime(DateTime dateTime)
        {
            var cmd = new SetDateTimeCommand(DeviceId, dateTime);
            await SendCommand(cmd);
        }

        public async Task<Statistic> GetDownloadInformation()
        {
            var response = await SendCommand(new GetRecordInfoCommand(DeviceId));
            return new Statistic(response.DATA);
        }

        public async Task<List<Record>> DownloadRecords(bool onlyNew = false)
        {
            var statistics = await GetDownloadInformation();
            var recordAmount = onlyNew ? statistics.NewRecordAmount : statistics.AllRecordAmount;
            var records = new List<Record>((int)recordAmount);
            var isFirst = true;
            while (recordAmount > 0)
            {
                var response = await SendCommand(new GetRecordsCommand(DeviceId, isFirst, onlyNew, recordAmount));
                var counter = response.DATA[0];
                recordAmount -= counter;
                for (var i = 0; i < counter; i++)
                {
                    records.Add(new Record(response.DATA, 1 + i * Record.RECORD_LENGTH));
                }
                isFirst = false;
            }
            return records;
        }

        public async Task SetRecords(Record record)
        {
            await SendCommand(new SetRecordsCommand(DeviceId, record));
        }

        public async Task ClearNewRecords()
        {
            await SendCommand(new ClearRecordsCommand(DeviceId, ClearRecordsCommand.CLEAR_ALL, 0));
        }

        public async Task ClearNewRecords(ulong amount)
        {
            await SendCommand(new ClearRecordsCommand(DeviceId, ClearRecordsCommand.CLEAR_AMOUNT, amount));
        }

        public async Task DeleteAllRecords()
        {
            await SendCommand(new ClearRecordsCommand(DeviceId, ClearRecordsCommand.DELETE_ALL, 0));
        }

        public async Task<List<UserInfo>> GetEmployeesData()
        {
            var statistics = await GetDownloadInformation();
            var userAmount = statistics.UserAmount;
            var users = new List<UserInfo>((int)userAmount);
            var isFirst = true;
            while (userAmount > 0)
            {
                var response = await SendCommand(new GetStaffDataCommand(DeviceId, isFirst, userAmount));
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

        public async Task SetEmployeesData(UserInfo user)
        {
            await SetEmployeesData(new List<UserInfo> { user });
        }

        public async Task SetEmployeesData(List<UserInfo> users)
        {
            while (users.Count > 0)
            {
                await SendCommand(new SetStaffDataCommand(DeviceId, users));
            }
        }

        public async Task DeleteEmployeesData(UserInfo user)
        {
            await DeleteEmployeesData(user.Id);
        }

        public async Task DeleteEmployeesData(ulong employeeID)
        {
            await SendCommand(new DeleteStaffDataCommand(DeviceId, employeeID));
        }

        public async Task<byte[]> GetFingerprintTemplate(ulong employeeID, Finger finger)
        {
            var response = await SendCommand(new GetFingerprintTemplateCommand(DeviceId, employeeID, finger));
            return response.DATA;
        }

        public async Task SetFingerprintTemplate(ulong employeeID, Finger finger, byte[] template)
        {
            await SendCommand(new SetFingerprintTemplateCommand(DeviceId, employeeID, finger, template));
        }

        public async Task<byte[]> EnrollFingerprint(ulong employeeID)
        {
            await SendCommand(new EnrollFingerprintCommand(DeviceId, employeeID, true));
            await SendCommand(new EnrollFingerprintCommand(DeviceId, employeeID, false));
            return await GetFingerprintTemplate(employeeID, 0);
        }

        public async Task<TcpParameters> GetTcpParameters()
        {
            var response = await SendCommand(new GetTCPParametersCommand(DeviceId));
            return new TcpParameters(response.DATA);
        }

        public async Task SetTCPParameters(TcpParameters value)
        {
            await SendCommand(new SetTCPParametersCommand(DeviceId, value));
        }

        public async Task<string> GetDeviceSN()
        {
            var response = await SendCommand(new GetDeviceSNCommand(DeviceId));
            return Bytes.GetAsciiString(response.DATA);
        }

        public async Task SetDeviceSN(string value)
        {
            await SendCommand(new SetDeviceSNCommand(DeviceId, value));
        }

        public async Task<ulong> GetDeviceID()
        {
            var response = await SendCommand(new GetDeviceIDCommand(DeviceId));
            DeviceId = Bytes.Read(response.DATA);
            return DeviceId;
        }

        public async Task SetDeviceID(ulong newDeviceId)
        {
            await SendCommand(new SetDeviceIDCommand(DeviceId, newDeviceId));
            if (DeviceId != 0)
            {
                DeviceId = newDeviceId;
            }
        }

        public async Task<string> GetDeviceTypeCode()
        {
            var response = await SendCommand(new GetDeviceTypeCommand(DeviceId));
            return Bytes.GetAsciiString(response.DATA);
        }

        public async Task RebootDevice()
        {
            await SendCommand(new RebootDeviceCommand(DeviceId));
        }

        public async Task ResetToFactorySettings()
        {
            await SendCommand(new ResetToFactorySettingsCommand(DeviceId));
        }

        public async Task UnlockDoor()
        {
            await SendCommand(new UnlockDoorCommand(DeviceId));
        }

        public async Task<BasicSettings> GetBasicSettings()
        {
            var response = await SendCommand(new GetBasicSettingsCommand(DeviceId));
            return new BasicSettings(response.DATA);
        }

        public async Task SetBasicSettings(BasicSettings value)
        {
            await SendCommand(new SetBasicSettingsCommand(DeviceId, value));
        }

        public async Task<AdvancedSettings> GetAdvancedSettings()
        {
            var response = await SendCommand(new GetAdvancedSettingsCommand(DeviceId));
            return new AdvancedSettings(response.DATA);
        }

        public async Task SetAdvancedSettings(AdvancedSettings value)
        {
            await SendCommand(new SetAdvancedSettingsCommand(DeviceId, value));
        }

        public void Dispose()
        {
            DeviceStream.Dispose();
            DeviceSocket.Close();
        }
    }
}
