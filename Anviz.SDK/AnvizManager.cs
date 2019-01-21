using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using Anviz.SDK.Utils;
using System;
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

        public Statistic GetDownloadInformation()
        {
            var cmd = new GetRecordInfoCommand(deviceId);
            cmd.Send(stream);
            var response = Response.FromStream(stream);
            return new Statistic(response.DATA);
        }

        public List<Record> DownloadRecords(uint recordAmount)
        {
            List<Record> records = new List<Record>();
            bool isFirst = true;
            DateTime defaultDate = new DateTime(2000, 01, 02, 0, 0, 0);
            while (recordAmount > 0)
            {
                var cmd = new GetRecordsCommand(deviceId, isFirst, recordAmount);
                cmd.Send(stream);
                var response = Response.FromStream(stream);
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
                var cmd = new GetStaffDataCommand(deviceId, isFirst, userAmount);
                cmd.Send(stream);
                var response = Response.FromStream(stream);
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
            var cmd = new GetTCPParametersCommand(deviceId);
            cmd.Send(stream);
            var response = Response.FromStream(stream);
            return new TcpParameters(response.DATA);
        }

        public ulong GetDeviceSN()
        {
            var cmd = new GetDeviceSNCommand(deviceId);
            cmd.Send(stream);
            var response = Response.FromStream(stream);
            return Bytes.Read(response.DATA);
        }

        public string GetDeviceTypeCode()
        {
            var cmd = new GetDeviceTypeCommand(deviceId);
            cmd.Send(stream);
            var response = Response.FromStream(stream);
            return Bytes.GetAsciiString(response.DATA);
        }

        public void ClearNewRecords()
        {
            var cmd = new ClearNewRecordsCommand(deviceId);
            cmd.Send(stream);
            Response.FromStream(stream);
        }
    }
}
