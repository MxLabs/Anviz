using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using Anviz.SDK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
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
            return new Statistic(stream);
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
                var values = new Response(stream);
                if (values.IsValid)
                {
                    int counter = values.DATA.First();
                    recordAmount -= (uint)counter;
                    values.DATA = Bytes.Split(values.DATA, 1, values.DATA.Length);
                    for (int i = 0; i < counter; i++)
                    {
                        int pos = i * 14;
                        Record record = new Record();
                        record.UserCode = Bytes.Read(Bytes.Split(values.DATA, pos, 5));
                        record.DateTime = Bytes.Read(Bytes.Split(values.DATA, pos + 5, 4));
                        record.BackupCode = values.DATA[pos + 10];
                        record.RecordType = values.DATA[pos + 11];
                        record.WorkType = (uint)Bytes.Read(Bytes.Split(values.DATA, pos + 12, 3));
                        records.Add(record);
                    }
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
                var values = new Response(stream);
                if (values.IsValid)
                {
                    uint counter = values.DATA.First();
                    userAmount -= counter;
                    values.DATA = Bytes.Split(values.DATA, 1, values.DATA.Length);
                    for (int i = 0; i < counter; i++)
                    {
                        int pos = i * 40;
                        UserInfo userInfo = new UserInfo();
                        userInfo.Id = Bytes.Read(Bytes.Split(values.DATA, pos, 5));
                        userInfo.Name = Bytes.GetString(Bytes.Split(values.DATA, pos + 12, 10));
                        users.Add(userInfo);
                    }
                }
                isFirst = false;
            }
            return users;
        }

        public TcpParameters GetTcpParameters()
        {
            var cmd = new GetTCPParametersCommand(deviceId);
            cmd.Send(stream);
            var parsed = new Response(stream);
            if (parsed.IsValid)
            {
                var parameters = new TcpParameters();
                parameters.IP = string.Join(".", Bytes.Split(parsed.DATA, 0, 4));
                parameters.SubnetMask = string.Join(".", Bytes.Split(parsed.DATA, 4, 4));
                parameters.MacAddress = string.Join("-", Bytes.Split(parsed.DATA, 8, 6));
                parameters.DefaultGateway = string.Join(".", Bytes.Split(parsed.DATA, 14, 4));
                parameters.Server = string.Join(".", Bytes.Split(parsed.DATA, 18, 4));
                parameters.FarLimit = parsed.DATA[23];
                parameters.ComPort = (int)Bytes.Read(Bytes.Split(parsed.DATA, 23, 2));
                parameters.TcpMode = parsed.DATA[25] == 0 ? "Server Mode" : "Client Mode";
                parameters.DhcpLimit = parsed.DATA[26];
                return parameters;
            }
            return null;
        }

        public ulong GetDeviceSN()
        {
            var cmd = new GetDeviceSNCommand(deviceId);
            cmd.Send(stream);
            var parsed = new Response(stream);
            if (parsed.IsValid)
            {
                return Bytes.Read(Bytes.Split(parsed.DATA, 0, 4));
            }
            return 0;
        }

        public string GetDeviceTypeCode()
        {
            var cmd = new GetDeviceTypeCommand(deviceId);
            cmd.Send(stream);
            var parsed = new Response(stream);
            if (parsed.IsValid)
            {
                return Bytes.GetString(Bytes.Split(parsed.DATA, 0, 8));
            }
            return string.Empty;
        }

        public bool ClearNewRecords()
        {
            var cmd = new ClearNewRecordsCommand(deviceId);
            cmd.Send(stream);
            var parsed = new Response(stream);
            if (parsed.IsValid)
            {
                return true;
            }
            return false;
        }
    }
}
