using System;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports; 
using System.Threading;

namespace net5Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            ProxCardReader reader = new ProxCardReader();
            reader.CardReceived += Reader_CardReceived;
            reader.InitWatcher();

            TimerCallback callback = new TimerCallback(Tick);
            _timer = new Timer(callback, null, new TimeSpan(0,0,0), new TimeSpan(0,0,1));

            Console.ReadLine();
        }

        private static void Reader_CardReceived(object sender, CardDataEventArgs e)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"Facility: {e.FacilityId}");
            sb.AppendLine($"Card:     {e.CardId}");
            sb.AppendLine($"{DateTime.Now}");

            Console.WriteLine(sb.ToString());
        }

        private static int Counter = 0;
        private static System.Threading.Timer _timer;
        private static void Tick(Object o) => Console.WriteLine(Counter++);
        

    }


}



            //List<string> portnames = SerialPort.GetPortNames().ToList();


            /*
            using (var searcher = new ManagementObjectSearcher
                ("SELECT * FROM WIN32_SerialPort"))
            {
                string[] portnames = SerialPort.GetPortNames();
                List<ManagementBaseObject> ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
                ports.ForEach(p => Console.WriteLine($"{p.ToString()}"));

                var tList = (from n in portnames
                            join p in ports on n equals p["DeviceID"].ToString()
                            select n + " - " + p["Caption"]).ToList();

                tList.ForEach(Console.WriteLine);
            }

            // pause program execution to review results...
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
            */

            //Win32_USBHub - multiple listed. the one i want isnt listed. 
            //Win32_SerialPort - 3 devices. the one i want isnt listed. 
            //Win32_USBControllerDevice - no results
            //Win32_PnPEntity

    /*
            string vid = "1A86";
            string pid = "7523";


            ManagementScope connectionScope = new ManagementScope();
            SelectQuery serialQuery = new SelectQuery($@"SELECT * FROM Win32_PnPEntity where DeviceID like '%VID_{vid}&PID_{pid}%'");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(connectionScope, serialQuery);

            int count = 0;
            foreach (ManagementObject item in searcher.Get())
            try
            {
                count++;
               // string descr = item.ToString();

                //if (descr.Contains("1A86"))
                //    Console.WriteLine(descr);


                    string desc = "";//item["Description"].ToString();
                    string deviceId = item["DeviceID"].ToString();
                    
                    if (desc.Contains("1A86") || deviceId.Contains("1A86"))
                        Console.WriteLine($"{deviceId} {desc}");



                    //if (desc.Contains("Arduino"))
                    //{
                    //    return deviceId;
                    //}
               
            }
            catch (ManagementException e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine($"{count} devices found");
*/