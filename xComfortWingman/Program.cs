using System;
using static xComfortWingman.MyLogger;

namespace xComfortWingman
{
    class Program
    {
        public static readonly Settings Settings = new Settings(true);
        public static readonly DateTime ApplicationStart = DateTime.Now;
        public static bool BootWithoutError = true;

        static void Main(string[] args)
        {
            DoLog("Starting BachelorPad...",4);

            BootWithoutError = CI.ImportDatapointsFromFile(Settings.DATAPOINTS_FILENAME);
            if (BootWithoutError) { MQTT.RunMQTTClientAsync().Wait(); }
            if (BootWithoutError) { CI.ConnectToCI().Wait(); }
            if (BootWithoutError)
            {
                DoLog("Startup complete!", 4);

                if (Settings.DEBUGMODE)
                {
                    DoLog("Press Enter to do some diagnostics!", 0);
                    Console.ReadLine();
                    CI.FakeData(new byte[] { 0x0C, 0xC1, 0x02, 0x70, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x40, 0x10, 0x00 }).Wait();
                    //CI.SendData(5, 30).Wait();
                    //Console.ReadLine();
                    //CI.SendData(5, 40).Wait();
                    //Console.ReadLine();
                    //CI.SendData(5, 50).Wait();
                    //Console.ReadLine();
                    //CI.SendData(5, 0).Wait();
                }

                while (true)
                {
                    // Just chill, other threads are monitoring communications...
                }
            }
            else
            {
                DoLog("Something failed during the program startup! Please check the logs for more info.",5);
            }
        }
    }
}
