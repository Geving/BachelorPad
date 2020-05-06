using System;
using System.Diagnostics;
using System.IO;
using static xComfortWingman.MyLogger;

namespace xComfortWingman
{
    class Program
    {
        public static readonly Settings Settings = new Settings(true);
        public static readonly DateTime ApplicationStart = DateTime.Now;
        public static bool BootWithoutError = true;
        private static bool IllegalArguments = false;
        private static bool DoExport = false;
        public static bool DoExit = false;
        public static bool StayAlive = true;
        static void Main(string[] args)
        {
            //Basic basic = new Basic();
            //Basic.Device newDevice = new Basic.Device("My dimmer", ,
            //Basic.Device myDev = new DevType.DimmingActuator();
            //Basic.Device device = new Basic.Device.Ds();


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
                    case "-e":
                        {
                            ExportDevicesToOpenHABformat();
                            break;
                        }
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

            Menu.MainMenu();
            if (DoExit) { return; }

            DoLog("Starting BachelorPad...",4);
            if (Settings.GENERAL_FROM_FILE == false) { DoLog("Using default settings!", 4); }

            //if(Settings.DEBUGMODE) { Console.WriteLine(Settings.GetSettingsAsJSON()); Console.ReadLine(); }

            // For easier switching between developer machine and the Raspberry Pi meant for production use.
            if (System.Net.Dns.GetHostName().ToUpper() == "ORION") 
            {
                Console.WriteLine("YOU ARE NOW RUNNING ON THE DEVELOPER MACHINE!");
                Settings.MQTT_BASETOPIC = "debugdata";
                if (ImportDatapointsFromFile("C:\\misc\\" + Settings.GENERAL_DATAPOINTS_FILENAME))
                {
                    CreateDevicesOutOfDatapoints();
                    MQTT.RunMQTTClientAsync().Wait();

                    MQTT.PublishDeviceAsync(Homie.CreateDeviceFromDatapoint(CI.datapoints[4])).Wait();
                    Console.WriteLine($"Publications: {MQTT.PublicationCounter}");
                    CI.FakeData(new byte[] { 0x0D, 0xC1, 0x05, 0x70, 0x00, 0x62, 0x00, 0x00, 0x00, 0x00, 0x32, 0x10, 0x0B }).Wait();
                    //CI.FakeData(new byte[] { 0x0D, 0xC1, 0x31, 0x62, 0x17, 0x00, 0x00, 0xC9, 0x00, 0x00, 0x44, 0x24, 0x01 }).Wait();
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
                return;
            };


            BootWithoutError = ImportDatapointsFromFile(Settings.GENERAL_DATAPOINTS_FILENAME);
            if (BootWithoutError) { CreateDevicesOutOfDatapoints(); }
            if (BootWithoutError) { MQTT.RunMQTTClientAsync().Wait(); }
            if (BootWithoutError) { CI.ConnectToCI().Wait(); }
            if (BootWithoutError)
            {
                while (StayAlive)
                {
                    // Just chill, other threads are monitoring communications...
                }
                // If this point in the code is reached, it means that a shutdown command has been given via MQTT.
                MQTT.DisconnectMQTTClientAsync().Wait();
            }
            else
            {
                DoLog("Something failed during the program startup! Please check the logs for more info.",5);
            }

            DoLog("Terminating...", 4);
        }

        #region "Datapoints stuff"
        public static bool ImportDatapointsFromFile(String filePath)
        {
            try
            {
                //Boilerplate - Read a datapoint file exported from Eatons own software
                /* Snippet from an actual file:
                 *  DP  DP+channel      Serial  Typ Ch  Mod Cls N/A
                    13	DblBathroomF0 	4925325 16	0	0	0	#000#000#000#000#0#000#000#005#000#	
                    14	DimToiletDown 	5057045	17	0	0	0	#000#000#000#000#0#000#000#006#000#	
                    15	DimBathroomDwn 	5027425	17	0	0	0	#000#000#000#000#0#000#000#006#000#	
                    16	DimInnerHallN 	3288803	17	0	0	0	#000#000#000#000#0#000#000#006#000#	
                    17	DimInnerHallS 	3812402	17	0	0	0	#000#000#000#000#0#000#000#006#000#	
                   Delimiter is tab
                 */

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                DoLog("Importing datapoints from file...", false);

                if (!File.Exists(filePath))
                {
                    DoLog("FAILED", 3, false, 12);
                    DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
                    stopwatch.Reset();
                    DoLog("Datapoint file not found!");
                    return false;
                }
                string aline;
                FileStream fileStream = new FileStream(filePath, FileMode.Open);
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    while ((aline = reader.ReadLine()) != null)
                    {
                        string[] line = aline.Split("\t");
                        CI.datapoints.Add(new Datapoint(Convert.ToInt32(line[0]), line[1].Trim(), Convert.ToInt32(line[2]), Convert.ToInt32(line[3]), Convert.ToInt32(line[4]), Convert.ToInt32(line[5]), Convert.ToInt32(line[6]), line[7]));
                        //DoLog("Added datapoint #" + line[0] + " named " + line[1]);
                    }
                }
                fileStream.Close();
                DoLog("OK", 3, false, 10);
                DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
                DoLog($"Total number of datapoints: ", false);
                DoLog($"{ CI.datapoints.Count}", 3, true, 10);
                stopwatch.Reset();

                return true;
            }
            catch (Exception exception)
            {
                LogException(exception);
                return false;
            }
        }

        public static bool ImportDatapointsOneByOne(String dataPointLine)
        {
            try
            {
                //Allows us to add a single datapoint through some other method than reading the file from disk.
                string[] line = dataPointLine.Split("\t");
                CI.datapoints.Add(new Datapoint(Convert.ToInt32(line[0]), line[1], Convert.ToInt32(line[2]), Convert.ToInt32(line[3]), Convert.ToInt32(line[4]), Convert.ToInt32(line[5]), Convert.ToInt32(line[6]), line[7]));
                return true;
            }
            catch (Exception exception)
            {
                LogException(exception);
                return false;
            }
        }


        public static String GetDatapointFile()
        {
            try
            {
                if (!File.Exists(Program.Settings.GENERAL_DATAPOINTS_FILENAME))
                {
                    DoLog("Datapoint file not found!");
                    return "File not found!";
                }
                string everything = "Empty file!";
                FileStream fileStream = new FileStream(Program.Settings.GENERAL_DATAPOINTS_FILENAME, FileMode.Open);
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    everything = reader.ReadToEnd();
                }
                fileStream.Close();
                return everything;
            }
            catch (Exception exception)
            {
                LogException(exception);
                return exception.Message;
            }
        }

        public static bool SetDatapointFile(String contents)
        {
            try
            {
                FileStream fileStream = new FileStream(Program.Settings.GENERAL_DATAPOINTS_FILENAME, FileMode.Create);
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    writer.Write(contents);
                }
                fileStream.Close();
                return true;
            }
            catch (Exception exception)
            {
                LogException(exception);
                return false;
            }
        }

        private static void CreateDevicesOutOfDatapoints()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            DoLog("Creating devices from datapoints...", false);
            foreach (Datapoint datapoint in CI.datapoints)
            {
                Homie.devices.Add(Homie.CreateDeviceFromDatapoint(datapoint));
            }
            DoLog("OK", 3, false, 10);
            DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
            stopwatch.Reset();
            DoLog($"Total number of devices: ", false);
            DoLog($"{ Homie.devices.Count}",3,true,10);
        }

        public static void ExportDevicesToOpenHABformat()
        {

            int itemCount = 0;
            string ItemChannelLinkJSON = "{";
            string CoreItemJSON = "{";
            ImportDatapointsFromFile(Settings.GENERAL_DATAPOINTS_FILENAME);
            CreateDevicesOutOfDatapoints();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            DoLog("Exporting devices to OpenHAB format...", false);
            foreach (Homie.Device device in Homie.devices)
            {
                ItemChannelLinkJSON += Export.GetItemChannelLinkJSONfromDevice(device) + ",";
                CoreItemJSON += Export.GetCoreThingJSONfromDevice(device) + ",";
                itemCount++;
            }
            ItemChannelLinkJSON = ItemChannelLinkJSON.Remove(ItemChannelLinkJSON.Length - 1) + "\n}";
            ItemChannelLinkJSON = ItemChannelLinkJSON.Remove(ItemChannelLinkJSON.Length - 1) + "\n}";

            using (StreamWriter w = new StreamWriter("ItemChannelLink.json"))
            {
                w.WriteLine(ItemChannelLinkJSON);
            }
            using (StreamWriter w = new StreamWriter("CoreItem.json"))
            {
                w.WriteLine(CoreItemJSON);
            }
            DoLog("OK", 3, false, 10);
            DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
            stopwatch.Reset();
            DoLog($"Total number of devices exported: ", false);
            DoLog($"{ itemCount }", 3, true, 10);
        }
        #endregion

    }
}
