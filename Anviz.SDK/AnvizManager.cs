using Anviz.SDK.Commands;
using Anviz.SDK.Responses;
using Anviz.SDK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
namespace Anviz.SDK
{
    public class AnvizManager
    {
        private readonly ulong deviceId;

        private const byte ACK_SUCCESS = 0x00;
        private const byte ACK_FAIL = 0x01;

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

        private byte[] SendCommand(Command cmd)
        {
            stream.Write(cmd.payload, 0, cmd.payload.Length);
            Thread.Sleep(400);
            if (socket.Available > 0)
            {
                byte[] receivedBytes = new byte[socket.Available];
                stream.Read(receivedBytes, 0, receivedBytes.Length);
                return receivedBytes;
            }
            return null;
        }

        public Statistic GetDownloadInformation()
        {
            byte[] response = SendCommand(new GetRecordInfoCommand(deviceId));
            Statistic deviceStatistic = null;
            if (response != null)
            {
                var parsed = new Response(response);
                deviceStatistic = new Statistic();
                deviceStatistic.UserAmount = (uint)Bytes.Read(Bytes.Split(parsed.DATA, 0, 3));
                deviceStatistic.FingerPrintAmount = (uint)Bytes.Read(Bytes.Split(parsed.DATA, 3, 3));
                deviceStatistic.PasswordAmount = (uint)Bytes.Read(Bytes.Split(parsed.DATA, 6, 3));
                deviceStatistic.CardAmount = (uint)Bytes.Read(Bytes.Split(parsed.DATA, 9, 3));
                deviceStatistic.AllRecordAmount = (uint)Bytes.Read(Bytes.Split(parsed.DATA, 12, 3));
                deviceStatistic.NewRecordAmount = (uint)Bytes.Read(Bytes.Split(parsed.DATA, 15, 3));
            }
            return deviceStatistic;
        }

        public List<Record> DownloadRecords(uint recordAmount)
        {
            List<Record> records = new List<Record>();
            bool isFirst = true;
            DateTime defaultDate = new DateTime(2000, 01, 02, 0, 0, 0);
            while (recordAmount > 0)
            {
                byte[] response = SendCommand(new GetRecordsCommand(deviceId, isFirst, recordAmount));
                if (response == null)
                {
                    return null;
                }
                var values = new Response(response);
                if (values.RET == ACK_SUCCESS)
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
                byte[] response = SendCommand(new GetStaffDataCommand(deviceId, isFirst, userAmount));
                if (response == null)
                {
                    return null;
                }
                var values = new Response(response);
                if (values.RET == ACK_SUCCESS)
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
            byte[] response = SendCommand(new GetTCPParametersCommand(deviceId));
            if (response != null)
            {
                var parsed = new Response(response);
                if (parsed.RET == ACK_SUCCESS)
                {
                    TcpParameters parameters = new TcpParameters();
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
            }
            return null;
        }

        public ulong GetDeviceSN()
        {
            byte[] response = SendCommand(new GetDeviceSNCommand(deviceId));
            if (response != null)
            {
                var parsed = new Response(response);
                if (parsed.RET == ACK_SUCCESS)
                {
                    return Bytes.Read(Bytes.Split(parsed.DATA, 0, 4));
                }
            }
            return 0;
        }

        public string GetDeviceTypeCode()
        {
            byte[] response = SendCommand(new GetDeviceTypeCommand(deviceId));
            if (response != null)
            {
                var parsed = new Response(response);
                if (parsed.RET == ACK_SUCCESS)
                {
                    return Bytes.GetString(Bytes.Split(parsed.DATA, 0, 8));
                }
            }
            return string.Empty;
        }

        public bool ClearNewRecords()
        {
            byte[] response = SendCommand(new ClearNewRecordsCommand(deviceId));
            if (response != null)
            {
                var parsed = new Response(response);
                if (parsed.RET == ACK_SUCCESS)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
