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
        private readonly ulong DeviceId;

        private readonly TcpClient DeviceSocket;
        private readonly NetworkStream DeviceStream;

        public AnvizDevice(TcpClient socket, ulong deviceId)
        {
            DeviceId = deviceId;
            DeviceSocket = socket;
            DeviceStream = socket.GetStream();
        }

        private async Task<Response> SendCommand(Command cmd)
        {
            await cmd.Send(DeviceStream);
            return await Response.FromStream(cmd.ResponseCode, DeviceStream);
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
                uint counter = response.DATA[0];
                recordAmount -= counter;
                for (int i = 0; i < counter; i++)
                {
                    records.Add(new Record(response.DATA, i * 14 + 1));
                }
                isFirst = false;
            }
            return records;
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
                uint counter = response.DATA[0];
                userAmount -= counter;
                for (int i = 0; i < counter; i++)
                {
                    users.Add(new UserInfo(response.DATA, i * 40 + 1));
                }
                isFirst = false;
            }
            return users;
        }

        public async Task<byte[]> GetFingerprintTemplate(ulong employeeID, Finger finger)
        {
            var response = await SendCommand(new GetFingerprintTemplateCommand(DeviceId, employeeID, finger));
            return response.DATA;
        }

        public async Task<TcpParameters> GetTcpParameters()
        {
            var response = await SendCommand(new GetTCPParametersCommand(DeviceId));
            return new TcpParameters(response.DATA);
        }

        public async Task<ulong> GetDeviceSN()
        {
            var response = await SendCommand(new GetDeviceSNCommand(DeviceId));
            return Bytes.Read(response.DATA);
        }

        public async Task<string> GetDeviceTypeCode()
        {
            var response = await SendCommand(new GetDeviceTypeCommand(DeviceId));
            return Bytes.GetAsciiString(response.DATA);
        }

        public async Task ClearNewRecords()
        {
            await SendCommand(new ClearNewRecordsCommand(DeviceId));
        }

        public void Dispose()
        {
            DeviceStream.Dispose();
            DeviceSocket.Close();
        }
    }
}
