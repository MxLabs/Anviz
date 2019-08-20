using Anviz.SDK;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sample
{
    class Program
    {
        private const ulong DEVICE_ID = 1;
        private const string DEVICE_HOST = "10.0.0.1";
        static async Task Main(string[] args)
        {
            var manager = new AnvizManager(0);
            using (var device = await manager.Connect(DEVICE_HOST))
            {
                var id = await device.GetDeviceID();
                var sn = await device.GetDeviceSN();
                var type = await device.GetDeviceTypeCode();
                Console.WriteLine($"Connected to device {type} ID {id} SN {sn}");
                if(id != DEVICE_ID)
                {
                    await device.SetDeviceID(DEVICE_ID);
                }
                var now = DateTime.Now;
                var deviceTime = await device.GetDateTime();
                Console.WriteLine($"Current device time is {deviceTime.ToShortDateString()} {deviceTime.ToShortTimeString()}");
                if (Math.Abs((now - deviceTime).TotalSeconds) > 1)
                {
                    await device.SetDateTime(now);
                    Console.WriteLine("Updated device time according to local time");
                }
                var net = await device.GetTcpParameters();
                Console.WriteLine($"Device IP is {net.IP} {net.SubnetMask} {net.DefaultGateway} {net.MacAddress} mode is {net.TcpMode.ToString()}");
#if false //here you can change network parameters
                net.DefaultGateway = IPAddress.Parse("10.0.0.5");
                await device.SetTCPParameters(net);
#endif
                var basic = await device.GetBasicSettings();
                Console.WriteLine($"FW {basic.Firmware} AdminPWD {basic.ManagementPassword} Vol {basic.Volume} DateFormat {basic.DateFormat} 24h {basic.Is24HourClock}");
#if false //here you can change basic parameters
                basic.Volume = 0;
                basic.DateFormat = Anviz.SDK.Responses.DateFormat.DDMMYY;
                basic.Is24HourClock = true;
                await device.SetBasicSettings(basic);
#endif
                var employees = await device.GetEmployeesData();
                var dict = new Dictionary<ulong, string>();
                foreach (var employee in employees)
                {
                    dict.Add(employee.Id, employee.Name);
                    Console.WriteLine($"Employee {employee.Id} -> {employee.Name} pwd {employee.Password} card {employee.Card} fp {string.Join(", ", employee.EnrolledFingerprints)}");
                    foreach (var f in employee.EnrolledFingerprints)
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
