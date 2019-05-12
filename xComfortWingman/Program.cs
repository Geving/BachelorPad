using System;
using static xComfortWingman.MyLogger;

namespace xComfortWingman
{
    class Program
    {
        public static readonly Settings Settings = new Settings(true);
        public static readonly DateTime ApplicationStart = DateTime.Now;
        public static bool BootWithoutError = true;
        //public static bool MQTTdone = false;

        static void Main(string[] args)
        {
            DoLog("Starting BachelorPad...",4);

            //if(Settings.DEBUGMODE) { Console.WriteLine(Settings.GetSettingsAsJSON()); Console.ReadLine(); }

            // For easier switching between developer machine and Raspberry Pi meant for production use.
            if (System.Net.Dns.GetHostName().ToUpper() == "ORION") 
            {
                Console.WriteLine("YOU ARE NOW RUNNING ON THE DEVELOPER MACHINE!");
                Settings.MQTT_BASETOPIC = "debugdata";
                if (CI.ImportDatapointsFromFile("C:\\misc\\" + Settings.DATAPOINTS_FILENAME))
                {
                    MQTT.RunMQTTClientAsync().Wait();
                    MQTT.SendInitialData().Wait();

                    //MQTT.SendMQTTMessageAsync("$nodes", "wheels,engine,lights[]", false).Wait();
                    //Homie.PublishDatapointAsNode(CI.datapoints[2]).Wait();
                    //Homie.PublishDatapointAsNode(CI.datapoints[3]).Wait();
                    //Homie.PublishDatapointAsNode(CI.datapoints[4]).Wait();
                    //Homie.PublishDatapointAsNode(CI.datapoints[5]).Wait();
                    //Homie.PublishDatapointAsNode(CI.datapoints[6]).Wait();

                    //MQTT.PublishDeviceData(CI.datapoints[4]).Wait();
                    //Console.WriteLine($"MQTT done:    {MQTTdone}");
                    Console.WriteLine($"Publications: {MQTT.PublicationCounter}");
                    //Console.WriteLine($"Confirmed:    {MQTT.ConfirmedPublications}");
                    //Console.WriteLine($"Failed:       {MQTT.FailedPublications}");
                    //Console.WriteLine($"BALANCE:      {MQTT.PublicationCounter-MQTT.ConfirmedPublications-MQTT.FailedPublications}");

                    CI.FakeData(new byte[] { 0x0D, 0xC1, 0x31, 0x62, 0x17, 0x00, 0x00, 0xC9, 0x00, 0x00, 0x44, 0x24, 0x01 }).Wait();
                    while (true)
                    {
                        // Nada!
                    };
                };
                Console.WriteLine("---------------------------------------------------------");
                while (true)
                {
                    // Nada!
                };
            };


            BootWithoutError = CI.ImportDatapointsFromFile(Settings.DATAPOINTS_FILENAME);
            if (BootWithoutError) { MQTT.RunMQTTClientAsync().Wait(); }
            if (BootWithoutError) { CI.ConnectToCI().Wait(); }
            if (BootWithoutError) { MQTT.SendMQTTMessageAsync("$state", "ready", true).Wait(); }
            if (BootWithoutError)
            {
                DoLog("Startup complete!", 4);

                if (Settings.DEBUGMODE)
                {
                    DoLog("Press Enter to do some diagnostics!", 0);
                    Console.ReadLine();
                    //CI.FakeData(new byte[] { 0x0C, 0xC1, 0x02, 0x70, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x40, 0x10, 0x00 }).Wait();
                    CI.FakeData(new byte[] { 0x0D, 0xC1, 0x31, 0x62, 0x17, 0x00, 0x00, 0xC9, 0x00, 0x00, 0x44, 0x24, 0x01 }).Wait();
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
