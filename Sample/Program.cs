using Anviz.SDK;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sample
{
    class Program
    {
        private const ulong DEVICE_ID = 0;
        private const string DEVICE_HOST = "10.0.0.1";
        static async Task Main(string[] args)
        {
            var manager = new AnvizManager(DEVICE_ID);
            using (var device = await manager.Connect(DEVICE_HOST))
            {
                var sn = await device.GetDeviceSN();
                var type = await device.GetDeviceTypeCode();
                Console.WriteLine($"Connected to device {type} with SN {sn.ToString()}");
                var now = DateTime.Now;
                var deviceTime = await device.GetDateTime();
                Console.WriteLine($"Current device time is {deviceTime.ToShortDateString()} {deviceTime.ToShortTimeString()}");
                if ((now - deviceTime).TotalSeconds > 1)
                {
                    await device.SetDateTime(now);
                    Console.WriteLine("Updated device time according to local time");
                }
                var net = await device.GetTcpParameters();
                Console.WriteLine($"Device IP is {net.IP} {net.SubnetMask} {net.DefaultGateway} {net.MacAddress} mode is {net.TcpMode.ToString()}");
                var employees = await device.GetEmployeesData();
                var dict = new Dictionary<ulong, string>();
                foreach (var employee in employees)
                {
                    dict.Add(employee.Id, employee.Name);
                    Console.WriteLine($"Employee {employee.Id} -> {employee.Name} pwd {employee.Password} card {employee.Card} fp {string.Join(", ", employee.EnrolledFingerprints)}");
                    foreach(var f in employee.EnrolledFingerprints)
                    {
                        var fp = await device.GetFingerprintTemplate(employee.Id, f);
                        Console.WriteLine($"-> {f} {Convert.ToBase64String(fp)}");
                    }
                }
                var records = await device.DownloadRecords(false); //true to get only new records
                foreach (var rec in records)
                {
                    Console.WriteLine($"Employee {dict[rec.UserCode]} at {rec.DateTime.ToLongDateString()} {rec.DateTime.ToLongTimeString()}");
                }
            }
            Console.ReadLine();
        }
    }
}
