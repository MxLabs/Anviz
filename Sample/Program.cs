using Anviz.SDK;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sample
{
    class Program
    {
        private const ulong DEVICE_ID = 1;
        private const string DEVICE_HOST = "192.168.0.10";
        static async Task Main(string[] args)
        {
            var manager = new AnvizManager(DEVICE_ID);
            using (var device = await manager.Connect(DEVICE_HOST))
            {
                var sn = await device.GetDeviceSN();
                var type = await device.GetDeviceTypeCode();
                Console.WriteLine($"Connected to device {type} with SN {sn.ToString()}");
                var employees = await device.GetEmployeesData();
                var dict = new Dictionary<ulong, string>();
                foreach (var employee in employees)
                {
                    dict.Add(employee.Id, employee.Name);
                    Console.WriteLine($"Employee {employee.Id} -> {employee.Name}");
                }
                var records = await device.DownloadRecords(false); //true to get only new records
                foreach (var rec in records)
                {
                    var t = ULongToDateTime(rec.DateTime);
                    Console.WriteLine($"Employee {dict[rec.UserCode]} at {t.ToLongDateString()} {t.ToLongTimeString()}");
                }
            }
        }

        private static DateTime ULongToDateTime(ulong value)
        {
            return new DateTime(2000, 1, 2).AddSeconds(value);
        }
    }
}
