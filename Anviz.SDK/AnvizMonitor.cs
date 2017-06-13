using System;
using System.Collections.Generic;

namespace Anviz.SDK
{
    public class AnvizMonitor : IDisposable
    {
        AnvizManager anvizManager = null;
        List<UserInfo> employees = null;
        Statistic stats = null;

        public AnvizMonitor(string host, ulong deviceId, int port = 5010)
        {
            anvizManager = new AnvizManager(host, deviceId, port, 1);   //used 1 seconds timeout
            anvizManager.Connect();
            InitializeEmployees();
        }

        public bool InitializeEmployees()
        {
            stats = anvizManager.GetDownloadInformation();
            if (stats != null)
            {
                employees = anvizManager.GetEmployeesData((int)stats.UserAmount);
                if (employees != null)
                {
                    employees.Sort();
                    return true;
                }
            }
            else
            {
                anvizManager.Connect();
            }
            return false;
        }

        public List<Record> GetNewEmployeeRecords()
        {
            stats = null;
            List<Record> records = null;
            stats = anvizManager.GetDownloadInformation();
            if (stats != null)
            {
                //there will be problems if 10k users arrive at once
                //stats.NewRecordAmount returns a very high number sometimes
                //stats.NewRecordAmount < 9999 is a temporary solution for the problem.
                if (stats.NewRecordAmount > 0 && stats.NewRecordAmount < 9999)
                {
                    records = anvizManager.DownloadRecords(stats.NewRecordAmount, true);
                    if (records != null)
                    {
                        if (!anvizManager.ClearNewRecords()) Console.WriteLine("An Error occured while clearing new records!");
                    }
                }
            }
            else
            {
                anvizManager.Connect();
            }
            return records;
        }

        public DateTime ConvertTimeToUTC(ulong d)
        {
            DateTime time = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            time = time.AddSeconds(d);
            return time;
        }

        public string GetEmployeeName(ulong id)
        {
            return employees[(int)id - 1].Name; //Employee IDs start from 1 on the device
        }

        public void Dispose()
        {
            anvizManager.Dispose();
        }
    }
}
