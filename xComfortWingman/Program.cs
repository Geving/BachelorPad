using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using static xComfortWingman.MyLogger;

namespace xComfortWingman
{
    class Program
    {
        public static Settings Settings = new Settings(true);
        public static readonly DateTime ApplicationStart = DateTime.Now;
        public static bool BootWithoutError = true;
        private static bool IllegalArguments = false;
        public static bool StayAlive = true;
        static void Main(string[] args)
        {
            //dotnet publish "C:\Users\harald.geving\Source\Repos\BachelorPad\" --configuration Release --framework netcoreapp3.1 --self-contained false --runtime linux-arm --verbosity quiet
            //clear; dotnet publish -r linux-arm -o \\192.168.0.3\c$\wwwpub\harald.geving.no\files

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
                    case "-nodebug": { Settings.GENERAL_DEBUGMODE = false; break; };
                    case "-ad0": { Settings.HOMEASSISTANT_DISCOVERYTOPIC = ""; break; };
                    case "-ad1": { Settings.HOMEASSISTANT_DISCOVERYTOPIC = "myhome"; break; };
                    case "-ad2": { Settings.HOMEASSISTANT_DISCOVERYTOPIC = "homeassistant"; break; };
                    case "-s": { Menu.ProcessGroup("all", true); break; }
                    //case "-clear": { if(HomeAssistant.ClearAutoConfig()==true) return; break; }
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

            //Menu.MainMenu();
            Console.BackgroundColor = ConsoleColor.Black;

            DoLog("Starting xComfort2MQTT bridge...",4);
            if (File.Exists("/mydata/settings.json"))
            {
                Settings.ReadSettingsFromFile();
            } 
            else
            {
                Settings.WriteSettingsToFile(Settings);
            }
            //if (Settings.GENERAL_FROM_FILE == false) { DoLog("Using default settings!", 4); }

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

                    MQTT.PublishHomieDeviceAsync(Homie.CreateDeviceFromDatapoint(CI.datapoints[4])).Wait();
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
            //if (BootWithoutError) { PublishAutoConfigForAllDevices(); }
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
                DoLog("Importing datapoints from '" + filePath + "'...", false);

                if (!File.Exists(filePath))
                {
                    DoLog("FAILED", 3, false, 12);
                    DoLog("Attempting download...", false);
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile("http://harald.geving.no/files/datenpunkte.txt", filePath);
                    if (!File.Exists(filePath)) 
                    { 
                        DoLog("FAILED", 3, false, 12);
                        DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
                        stopwatch.Reset();
                        DoLog("Datapoint file not found!");
                        return false;
                    }
                    else
                    {
                        DoLog("OK", 3, false, 10);
                        DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
                        stopwatch.Reset();
                    }
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
                HomeAssistant.SetupNewDevice(datapoint);
            }
            DoLog("OK", 3, false, 10);
            DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
            stopwatch.Reset();
            DoLog($"Total number of devices: ", false);
            DoLog($"{ Homie.devices.Count}",3,true,10);
        }
        #endregion

    }
}
