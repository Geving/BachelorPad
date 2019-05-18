using System;
using static xComfortWingman.MyLogger;

namespace xComfortWingman
{
    class Program
    {
        public static readonly Settings Settings = new Settings(true);
        public static readonly DateTime ApplicationStart = DateTime.Now;
        public static bool BootWithoutError = true;
        private static bool IllegalArguments = false;
        public static bool StayAlive = true;
        static void Main(string[] args)
        {
            // Handling CLI arguments
            foreach (string arg in args)
            {
                bool mm = false;
                bool im = false;
                switch (arg)
                {
                    case "-?":
                    case "--?":
                    case "--h":
                    case "/h":
                    case "/?":
                    case "-m": { mm = true; break; };
                    case "-h": { im=true; IllegalArguments = true; break; };
                    case "-def": { Settings.DefaultSettings(); break; };
                    case "-nope": { ; break; };
                    case "-debug": { Settings.GENERAL_DEBUGMODE = true; break; };
                    default:
                        {
                            Console.WriteLine("Unknown argument: " + arg);
                            Console.WriteLine("Try -h for a list of available arguments.");
                            IllegalArguments = true;
                            break;
                        }
                }
                if (mm) Menu.MainMenu();
                if (im) Menu.InfoMenu();

            }
            if (IllegalArguments) { return; }


            DoLog("Starting BachelorPad...",4);

            //if(Settings.DEBUGMODE) { Console.WriteLine(Settings.GetSettingsAsJSON()); Console.ReadLine(); }

            // For easier switching between developer machine and the Raspberry Pi meant for production use.
            if (System.Net.Dns.GetHostName().ToUpper() == "ORION") 
            {
                Console.WriteLine("YOU ARE NOW RUNNING ON THE DEVELOPER MACHINE!");
                Settings.MQTT_BASETOPIC = "debugdata";
                if (CI.ImportDatapointsFromFile("C:\\misc\\" + Settings.GENERAL_DATAPOINTS_FILENAME))
                {
                    MQTT.RunMQTTClientAsync().Wait();
                    MQTT.PublishDeviceAsync(Homie.GetDeviceFromDatapoint(CI.datapoints[2])).Wait();
                    Console.WriteLine($"Publications: {MQTT.PublicationCounter}");

                   // CI.FakeData(new byte[] { 0x0D, 0xC1, 0x31, 0x62, 0x17, 0x00, 0x00, 0xC9, 0x00, 0x00, 0x44, 0x24, 0x01 }).Wait();
                    while (StayAlive)
                    {
                        // Nada!
                    };
                };
                Console.WriteLine("---------------------------------------------------------");
                while (StayAlive)
                {
                    // Nada!
                };
            };


            BootWithoutError = CI.ImportDatapointsFromFile(Settings.GENERAL_DATAPOINTS_FILENAME);
            if (BootWithoutError) { MQTT.RunMQTTClientAsync().Wait(); }
            if (BootWithoutError) { CI.ConnectToCI().Wait(); }
            if (BootWithoutError)
            {
                DoLog("Startup complete!", 4);
                while (StayAlive)
                {
                    // Just chill, other threads are monitoring communications...
                }
                // If this point in the code is reached, it means that a shutdown command has been given via MQTT.
                
            }
            else
            {
                DoLog("Something failed during the program startup! Please check the logs for more info.",5);
            }
        }

        
    }
}
