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
        
        private ushort[] CRCTABLE = new ushort[]{
             0x0000,0x1189,0x2312,0x329B,0x4624,0x57AD,0x6536,0x74BF,0x8C48,0x9DC1,
             0xAF5A,0xBED3,0xCA6C,0xDBE5,0xE97E,0xF8F7,0x1081,0x0108,0x3393,0x221A,
             0x56A5,0x472C,0x75B7,0x643E,0x9CC9,0x8D40,0xBFDB,0xAE52,0xDAED,0xCB64,
             0xF9FF,0xE876,0x2102,0x308B,0x0210,0x1399,0x6726,0x76AF,0x4434,0x55BD,
             0xAD4A,0xBCC3,0x8E58,0x9FD1,0xEB6E,0xFAE7,0xC87C,0xD9F5,0x3183,0x200A,
             0x1291,0x0318,0x77A7,0x662E,0x54B5,0x453C,0xBDCB,0xAC42,0x9ED9,0x8F50,
             0xFBEF,0xEA66,0xD8FD,0xC974,0x4204,0x538D,0x6116,0x709F,0x0420,0x15A9,
             0x2732,0x36BB,0xCE4C,0xDFC5,0xED5E,0xFCD7,0x8868,0x99E1,0xAB7A,0xBAF3,
             0x5285,0x430C,0x7197,0x601E,0x14A1,0x0528,0x37B3,0x263A,0xDECD,0xCF44,
             0xFDDF,0xEC56,0x98E9,0x8960,0xBBFB,0xAA72,0x6306,0x728F,0x4014,0x519D,
             0x2522,0x34AB,0x0630,0x17B9,0xEF4E,0xFEC7,0xCC5C,0xDDD5,0xA96A,0xB8E3,
             0x8A78,0x9BF1,0x7387,0x620E,0x5095,0x411C,0x35A3,0x242A,0x16B1,0x0738,
             0xFFCF,0xEE46,0xDCDD,0xCD54,0xB9EB,0xA862,0x9AF9,0x8B70,0x8408,0x9581,
             0xA71A,0xB693,0xC22C,0xD3A5,0xE13E,0xF0B7,0x0840,0x19C9,0x2B52,0x3ADB,
             0x4E64,0x5FED,0x6D76,0x7CFF,0x9489,0x8500,0xB79B,0xA612,0xD2AD,0xC324,
             0xF1BF,0xE036,0x18C1,0x0948,0x3BD3,0x2A5A,0x5EE5,0x4F6C,0x7DF7,0x6C7E,
             0xA50A,0xB483,0x8618,0x9791,0xE32E,0xF2A7,0xC03C,0xD1B5,0x2942,0x38CB,
             0x0A50,0x1BD9,0x6F66,0x7EEF,0x4C74,0x5DFD,0xB58B,0xA402,0x9699,0x8710,
             0xF3AF,0xE226,0xD0BD,0xC134,0x39C3,0x284A,0x1AD1,0x0B58,0x7FE7,0x6E6E,
             0x5CF5,0x4D7C,0xC60C,0xD785,0xE51E,0xF497,0x8028,0x91A1,0xA33A,0xB2B3,
             0x4A44,0x5BCD,0x6956,0x78DF,0x0C60,0x1DE9,0x2F72,0x3EFB,0xD68D,0xC704,
             0xF59F,0xE416,0x90A9,0x8120,0xB3BB,0xA232,0x5AC5,0x4B4C,0x79D7,0x685E,
             0x1CE1,0x0D68,0x3FF3,0x2E7A,0xE70E,0xF687,0xC41C,0xD595,0xA12A,0xB0A3,
             0x8238,0x93B1,0x6B46,0x7ACF,0x4854,0x59DD,0x2D62,0x3CEB,0x0E70,0x1FF9,
             0xF78F,0xE606,0xD49D,0xC514,0xB1AB,0xA022,0x92B9,0x8330,0x7BC7,0x6A4E,
             0x58D5,0x495C,0x3DE3,0x2C6A,0x1EF1,0x0F78
        };
        private ulong deviceId = 0;
        private Socket socket = null;
        public AnvizManager(string host, ulong deviceId, int port = 5010)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(IPAddress.Parse(host), port));
            this.deviceId = deviceId;
        }

        private ushort CRC16(byte[] data)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < data.Length; i++)
            {
                var temp = (ushort)(crc ^ data[i]);
                crc = (ushort)((crc >> 8) ^ CRCTABLE[temp & 0x00FF]);
            }
            return crc;
        }
        private ulong ReadBytes(byte[] data)
        {
            ulong result = 0;
            for (int i = 0; i < data.Length; i++)
            {
                ulong b = (ulong)(data[data.Length - 1 - i] % 256);
                result += (ulong)(b << (byte)(i * 8));
            }
            return result;
        }
        private byte[] SplitBytes(byte[] data, int start, int count)
        {
            return data.Skip(start).Take(count).ToArray();
        }
        private byte[] SendCommand(byte command, ulong deviceId, byte[] data, ushort dataLength)
        {
            ushort crc = 0x0000;
            byte[] commandBytes = new byte[8 + data.Length];
            commandBytes[0] = 0xA5;
            commandBytes[1] = (byte)((deviceId >> 24) % 256);
            commandBytes[2] = (byte)((deviceId >> 16) % 256);
            commandBytes[3] = (byte)((deviceId >> 8) % 256);
            commandBytes[4] = (byte)(deviceId % 256);
            commandBytes[5] = command;
            commandBytes[6] = (byte)(dataLength >> 8);
            commandBytes[7] = (byte)(dataLength % 256);
            for (int i = 0; i < data.Length; i++)
            {
                commandBytes[i + 8] = data[i];
            }
            crc = CRC16(commandBytes);
            byte[] payload = new byte[commandBytes.Length + 2];
            Array.Copy(commandBytes, payload, commandBytes.Length);
            payload[9 + data.Length] = (byte)((crc >> 8) % 256);
            payload[8 + data.Length] = (byte)(crc % 256);
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
            byte[] response = SendCommand(GET_RECORD_INFO, deviceId, new byte[] { }, 0);
            Response parsed = GenerateResponse(response);
            Statistic deviceStatistic = new Statistic();
            deviceStatistic.UserAmount = (uint)ReadBytes(SplitBytes(parsed.DATA, 0, 3));
            deviceStatistic.FingerPrintAmount = (uint)ReadBytes(SplitBytes(parsed.DATA, 3, 3));
            deviceStatistic.PasswordAmount = (uint)ReadBytes(SplitBytes(parsed.DATA, 6, 3));
            deviceStatistic.CardAmount = (uint)ReadBytes(SplitBytes(parsed.DATA, 9, 3));
            deviceStatistic.AllRecordAmount = (uint)ReadBytes(SplitBytes(parsed.DATA, 12, 3));
            deviceStatistic.NewRecordAmount = (uint)ReadBytes(SplitBytes(parsed.DATA, 15, 3));
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
                byte[] response = SendCommand(GET_ALL_RECORDS, deviceId, data, 2);                
                Response values = GenerateResponse(response);
                if (values.RET == ACK_SUCCESS)
                {
                    int counter = values.DATA.First();
                    recordAmount -= (uint)counter;
                    values.DATA = SplitBytes(values.DATA, 1, values.DATA.Length);
                    for (int i = 0; i < counter; i++)
                    {
                        int pos = i * 14;
                        Record record = new Record();
                        record.UserCode = ReadBytes(SplitBytes(values.DATA, pos, 5));
                        record.DateTime = ReadBytes(SplitBytes(values.DATA, pos + 5, 4));
                        record.BackupCode = values.DATA[pos + 10];
                        record.RecordType = values.DATA[pos + 11];
                        record.WorkType = (uint)ReadBytes(SplitBytes(values.DATA, pos + 12, 3));
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
                byte[] data = new byte[] { package, 12 };
                byte[] response = SendCommand(GET_STAFF_DATA, deviceId, data, 2);
                Response values = GenerateResponse(response);
                if (values.RET == ACK_SUCCESS)
                {
                    int counter = values.DATA.First();
                    userAmount -= counter;
                    values.DATA = SplitBytes(values.DATA, 1, values.DATA.Length);
                    for (int i = 0; i < counter; i++)
                    {
                        int pos = i * 30;
                        UserInfo userInfo = new UserInfo();
                        userInfo.Id = ReadBytes(SplitBytes(values.DATA, pos, 5));
                        userInfo.Name = Encoding.UTF8.GetString(SplitBytes(values.DATA, pos + 12, 10));
                        users.Add(userInfo);
                    }
                }
                isFirst = false;
            }
            return users;
        }

        public TcpParameters GetTcpParameters()
        {
            byte[] response = SendCommand(GET_TCP_PARAMETERS, deviceId, new byte[] { }, 0);
            Response parsed = GenerateResponse(response);
            if (parsed.RET == ACK_SUCCESS)
            {
                TcpParameters parameters = new TcpParameters();
                parameters.IP = string.Join(".", SplitBytes(parsed.DATA, 0, 4));
                parameters.SubnetMask = string.Join(".", SplitBytes(parsed.DATA, 4, 4));
                parameters.MacAddress = string.Join("-", SplitBytes(parsed.DATA, 8, 6));
                parameters.DefaultGateway = string.Join(".", SplitBytes(parsed.DATA, 14, 4));
                parameters.Server = string.Join(".", SplitBytes(parsed.DATA, 18, 4));
                parameters.FarLimit = parsed.DATA[23];
                parameters.ComPort = (int)ReadBytes(SplitBytes(parsed.DATA, 23, 2));
                parameters.TcpMode = parsed.DATA[26] == 0 ? "Server Mode" : "Client Mode";
                parameters.DhcpLimit = parsed.DATA[27];
                return parameters;
            }
            return null;
        }

        public ulong GetDeviceSN()
        {
            byte[] response = SendCommand(GET_DEVICE_SN, deviceId, new byte[] { }, 0);
            Response parsed = GenerateResponse(response);
            if (parsed.RET == ACK_SUCCESS)
            {
                return ReadBytes(SplitBytes(parsed.DATA, 0, 4));
            }
            return 0;
        }

        public string GetDeviceTypeCode()
        {
            byte[] response = SendCommand(GET_DEVICE_TYPE, deviceId, new byte[] { }, 0);
            Response parsed = GenerateResponse(response);
            if (parsed.RET == ACK_SUCCESS)
            {
                return Encoding.UTF8.GetString(SplitBytes(parsed.DATA, 0, 8)).Replace("\0", "");
            }
            return string.Empty;
        }
    }
}
