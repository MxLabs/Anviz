using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using Anviz.SDK.Utils;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Anviz.SDK
{
    public class AnvizManager
    {
        private readonly ulong deviceId;

        private TcpClient socket;
        private NetworkStream stream;

        public AnvizManager(ulong deviceId)
        {
            this.deviceId = deviceId;
            socket = new TcpClient();
        }

        public async Task Connect(string host, int port = 5010)
        {
            await socket.ConnectAsync(host, port);
            stream = socket.GetStream();
        }

        private async Task<Response> SendCommand(Command cmd)
        {
            await cmd.Send(stream);
            return await Response.FromStream(stream);
        }

        public async Task<Statistic> GetDownloadInformation()
        {
            var response = await SendCommand(new GetRecordInfoCommand(deviceId));
            return new Statistic(response.DATA);
        }

        public async Task<List<Record>> DownloadRecords(bool onlyNew = false)
        {
            var statistics = await GetDownloadInformation();
            uint recordAmount;
            if (onlyNew)
            {
                recordAmount = statistics.NewRecordAmount;
            }
            else
            {
                recordAmount = statistics.AllRecordAmount;
            }
            List<Record> records = new List<Record>();
            bool isFirst = true;
            while (recordAmount > 0)
            {
                var response = await SendCommand(new GetRecordsCommand(deviceId, isFirst, onlyNew, recordAmount));
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
            List<UserInfo> users = new List<UserInfo>();
            bool isFirst = true;
            while (userAmount > 0)
            {
                var response = await SendCommand(new GetStaffDataCommand(deviceId, isFirst, userAmount));
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

        public async Task<TcpParameters> GetTcpParameters()
        {
            var response = await SendCommand(new GetTCPParametersCommand(deviceId));
            return new TcpParameters(response.DATA);
        }

        public async Task<ulong> GetDeviceSN()
        {
            var response = await SendCommand(new GetDeviceSNCommand(deviceId));
            return Bytes.Read(response.DATA);
        }

        public async Task<string> GetDeviceTypeCode()
        {
            var response = await SendCommand(new GetDeviceTypeCommand(deviceId));
            return Bytes.GetAsciiString(response.DATA);
        }

        public async Task ClearNewRecords()
        {
            await SendCommand(new ClearNewRecordsCommand(deviceId));
        }
    }
}
