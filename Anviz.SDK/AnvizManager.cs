using Anviz.SDK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
namespace Anviz.SDK
{
    public class AnvizManager
    {
        private const byte ACK_SUCCESS = 0x00;
        private const byte ACK_FAIL = 0x01;
        private const byte GET_RECORD_INFO = 0x3C;
        private const byte GET_ALL_RECORDS = 0x40;
        private const byte GET_DEVICE_SN = 0x46;
        private const byte GET_DEVICE_TYPE = 0x48;
        private const byte GET_STAFF_DATA = 0x72;
        private const byte GET_TCP_PARAMETERS = 0x3A;
        private const byte CLEAR_NEW_RECORDS = 0x4E;
        private ulong deviceId = 0;
        private Socket socket = null;
        private string host;
        private int port;
        public AnvizManager(string host, ulong deviceId, int port = 5010)
        {
            this.host = host;
            this.port = port;
            this.deviceId = deviceId;
        }

        public bool Connect()
        {
            try
            {
                if (socket != null) socket.Dispose();
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(new IPEndPoint(IPAddress.Parse(host), port));
                return true;
            }
            catch (SocketException e)
            {
                Console.WriteLine(string.Format("{0} Error code: {1}", e.Message, e.ErrorCode));
            }
            return false;
        }
        private byte[] SendCommand(byte command, ulong deviceId, byte[] data)
        {
            ushort crc = 0x0000;
            ushort dataLength = (ushort)data.Length;
            byte[] commandBytes = new byte[8 + dataLength];
            commandBytes[0] = 0xA5;
            commandBytes[1] = (byte)((deviceId >> 24) % 256);
            commandBytes[2] = (byte)((deviceId >> 16) % 256);
            commandBytes[3] = (byte)((deviceId >> 8) % 256);
            commandBytes[4] = (byte)(deviceId % 256);
            commandBytes[5] = command;
            commandBytes[6] = (byte)(dataLength >> 8);
            commandBytes[7] = (byte)(dataLength % 256);
            for (int i = 0; i < dataLength; i++)
            {
                commandBytes[i + 8] = data[i];
            }
            crc = CRC16.Compute(commandBytes);
            byte[] payload = new byte[commandBytes.Length + 2];
            Array.Copy(commandBytes, payload, commandBytes.Length);
            payload[9 + dataLength] = (byte)((crc >> 8) % 256);
            payload[8 + dataLength] = (byte)(crc % 256);
            socket.Send(payload);
            Thread.Sleep(400);
            if (socket.Available > 0)
            {
                byte[] receivedBytes = new byte[socket.Available];
                socket.Receive(receivedBytes);
                return receivedBytes;
            }
            return null;
        }

        private Response GenerateResponse(byte[] data)
        {
            Response response = null;
            if (data != null)
            {
                response = new Response();
                response.STX = data.Take(1).First();
                response.CH = data.Skip(1).Take(4).ToArray();
                response.ACK = data.Skip(5).Take(1).First();
                response.RET = data.Skip(6).Take(1).First();
                response.LEN = data.Skip(7).Take(2).ToArray();
                response.DATA = data.Skip(9).Take(data.Length - 2).ToArray();
                response.CRC = data.Skip(data.Length - 2).Take(2).ToArray();
            }
            return response;
        }
        public Statistic GetDownloadInformation()
        {
            byte[] response = SendCommand(GET_RECORD_INFO, deviceId, new byte[] { });
            Statistic deviceStatistic = null;
            if (response != null)
            {
                Response parsed = GenerateResponse(response);
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

                byte package = (byte)(isFirst ? 1 : 0);
                byte[] data = new byte[] { package, 12 };
                byte[] response = SendCommand(GET_ALL_RECORDS, deviceId, data);
                if (response == null)
                {
                    return null;
                }
                Response values = GenerateResponse(response);
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
        public List<UserInfo> GetEmployeesData(int userAmount)
        {
            List<UserInfo> users = new List<UserInfo>();
            bool isFirst = true;
            while (userAmount > 0)
            {
                byte package = (byte)(isFirst ? 1 : 0);
                byte[] data = new byte[] { package, 8 };
                byte[] response = SendCommand(GET_STAFF_DATA, deviceId, data);
                if (response == null)
                {
                    return null;
                }
                Response values = GenerateResponse(response);
                if (values.RET == ACK_SUCCESS)
                {
                    int counter = values.DATA.First();
                    userAmount -= counter;
                    values.DATA = Bytes.Split(values.DATA, 1, values.DATA.Length);
                    for (int i = 0; i < counter; i++)
                    {
                        int pos = i * 40;
                        UserInfo userInfo = new UserInfo();
                        userInfo.Id = Bytes.Read(Bytes.Split(values.DATA, pos, 5));
                        userInfo.Name = Encoding.BigEndianUnicode.GetString(Bytes.Split(values.DATA, pos + 12, 10)).TrimEnd('\0');
                        users.Add(userInfo);
                    }
                }
                isFirst = false;
            }
            return users;
        }

        public TcpParameters GetTcpParameters()
        {
            byte[] response = SendCommand(GET_TCP_PARAMETERS, deviceId, new byte[] { });
            if (response != null)
            {
                Response parsed = GenerateResponse(response);
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
            byte[] response = SendCommand(GET_DEVICE_SN, deviceId, new byte[] { });
            if (response != null)
            {
                Response parsed = GenerateResponse(response);
                if (parsed.RET == ACK_SUCCESS)
                {
                    return Bytes.Read(Bytes.Split(parsed.DATA, 0, 4));
                }
            }
            return 0;
        }

        public string GetDeviceTypeCode()
        {
            byte[] response = SendCommand(GET_DEVICE_TYPE, deviceId, new byte[] { });
            if (response != null)
            {
                Response parsed = GenerateResponse(response);
                if (parsed.RET == ACK_SUCCESS)
                {
                    return Encoding.BigEndianUnicode.GetString(Bytes.Split(parsed.DATA, 0, 8)).TrimEnd('\0');
                }
            }
            return string.Empty;
        }

        public bool ClearNewRecords()
        {
            byte[] data = new byte[] { 1 };
            byte[] response = SendCommand(CLEAR_NEW_RECORDS, deviceId, data);
            if (response != null)
            {
                Response parsed = GenerateResponse(response);
                if (parsed.RET == ACK_SUCCESS)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
