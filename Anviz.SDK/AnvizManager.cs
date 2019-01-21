using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using Anviz.SDK.Utils;
using System.Collections.Generic;
using System.Net.Sockets;
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

        public void Connect(string host, int port = 5010)
        {
            socket.Connect(host, port);
            stream = socket.GetStream();
        }

        private Response SendCommand(Command cmd)
        {
            cmd.Send(stream);
            return Response.FromStream(stream);
        }

        public Statistic GetDownloadInformation()
        {
            var response = SendCommand(new GetRecordInfoCommand(deviceId));
            return new Statistic(response.DATA);
        }

        public List<Record> DownloadRecords(uint recordAmount)
        {
            List<Record> records = new List<Record>();
            bool isFirst = true;
            while (recordAmount > 0)
            {
                var response = SendCommand(new GetRecordsCommand(deviceId, isFirst, recordAmount));
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
        public List<UserInfo> GetEmployeesData(uint userAmount)
        {
            List<UserInfo> users = new List<UserInfo>();
            bool isFirst = true;
            while (userAmount > 0)
            {
                var response = SendCommand(new GetStaffDataCommand(deviceId, isFirst, userAmount));
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

        public TcpParameters GetTcpParameters()
        {
            var response = SendCommand(new GetTCPParametersCommand(deviceId));
            return new TcpParameters(response.DATA);
        }

        public ulong GetDeviceSN()
        {
            var response = SendCommand(new GetDeviceSNCommand(deviceId));
            return Bytes.Read(response.DATA);
        }

        public string GetDeviceTypeCode()
        {
            var response = SendCommand(new GetDeviceTypeCommand(deviceId));
            return Bytes.GetAsciiString(response.DATA);
        }

        public void ClearNewRecords()
        {
            SendCommand(new ClearNewRecordsCommand(deviceId));
        }
    }
}
