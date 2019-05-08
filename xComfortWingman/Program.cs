//#define LIBUSB

using System;
using System.Collections.Generic;
using xComfortWingman.Protocol;
using static xComfortWingman.Protocol.PT_RX.MGW_RX_MSG_TYPE;
using static xComfortWingman.Protocol.PT_RX.MGW_RX_DATA_TYPE;
using static xComfortWingman.Protocol.MGW_TYPE;
using System.IO;

using System.IO.Ports;
using System.Threading.Tasks;
using Device.Net;
using System.Diagnostics;
using Hid.Net.Windows;
using MQTTnet;
using MQTTnet.Client;
using System.Text;
using MQTTnet.Protocol;
using System.Linq;
using Usb.Net.Windows;
using Device.Net.LibUsb;

namespace xComfortWingman
{
    class Program
    {
        private static DebugLogger Logger = new DebugLogger();
        private static readonly Settings Settings = new Settings(true);

        static bool readyToTransmit = false;
        static bool bootComplete = false;

        public static IMqttClient mqttClient;
#if LIBUSB
        public static IDevice myDevice;
        //public static LibUsbDotNet.UsbDevice myDevice;
#else
        public static IDevice myDevice;
#endif
        public static List<Datapoint> datapoints;
        public static List<DeviceType> devicetypes;
        public static List<byte> receivedData = new List<byte>();
        public static bool acceptingData = false;

        public static byte sequenceCounter = 0x00;
        public static byte[][] messageHistory = new byte[15][];

        static void Main(string[] args)
        {
            Logger.Log("Test logger","Region?",null,LogLevel.Information);
            datapoints = new List<Datapoint>();
            DeviceTypeList dtl = new DeviceTypeList();
            devicetypes = dtl.ListDeviceTypes();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Hi, I'm your xComfort Wingman!");
            Console.WriteLine("I'm here to talk to xComfort for you.");
            Console.WriteLine();
            Console.WriteLine("You talk to me using MQTT, and I'll talk to xComfort by using a Communication Interface (CKOZ-00/03 or CKOZ-00/14)");
            Console.WriteLine("The default topic beginning is 'BacheclorPad/xComfort/', and can be changed in the settings.");
            Console.WriteLine();
            Console.WriteLine("Topics I subscribe to:           \t Topic is used for:");
            Console.WriteLine("\t  BacheclorPad/xComfort/cmd/X/ \t\t Listening for instructions for datapoint X (X is a number)");
            Console.WriteLine("\t  BacheclorPad/xComfort/get/X/ \t\t Requesting an updated status from datapoint X (X is a number)");
            Console.WriteLine("\t* BacheclorPad/xComfort/RAW/in/\t\t Sends the payload as RAW data directly to the interface."); 
            Console.WriteLine();
            Console.WriteLine("Topics I publish to:             \t I publish when:");
            Console.WriteLine("\t  BacheclorPad/xComfort/+/set/ \t\t When a device broadcasts data without receiving an instruction first.");
            Console.WriteLine("\t  BacheclorPad/xComfort/+/ack/ \t\t When confirmation of a completed instruction is received.");
            Console.WriteLine("\t* BacheclorPad/xComfort/RAW/   \t\t Reports all RAW data from the interface as it arrives.");
            Console.WriteLine();
            Console.WriteLine("\t* [RAW]\n\t  [This feature can be enabled or disabled in the settings.]");
            Console.WriteLine("\t  [The data is formatted as a human readable string of HEX values with space between each value.]");
            Console.WriteLine("\t  [Example: 06 1B 01 0A 01 00");
            Console.ForegroundColor = ConsoleColor.Gray;
            //Console.WriteLine("Current timeout value: " + Settings.RMF_TIMEOUT);

            ConnectToHIDAsync(); //Connecting to CI via USB
            //DoAllTheStuff();
   
            while (true){
                //Do nothing...
            }
        }

        public async static void DoAllTheStuff()
        {
            //ImportDatapoints();
            ImportDatapointsFromFile("Datenpunkte.txt");

            //Communications
            await RunMQTTClientAsync(); //Connecting to MQTT
            if (Settings.CONNECTION_MODE == CI_CONNECTION_MODE.USB_MODE)
            {
                await ConnectToHIDAsync(); //Connecting to CI via USB
            }
            else
            {
                openSerialport(); //Connecting to CI via RS232
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Startup complete!");
            Console.ForegroundColor = ConsoleColor.Gray;

            if (Settings.DEBUGMODE)
            {
                while (!bootComplete)
                {
                    //Do nothing but wait...
                }
                while (!readyToTransmit)
                {
                    //Do nothing but wait...
                }
                Console.WriteLine("Press Enter to do some diagnostics!");
                Console.ReadLine();
                SendDataToDP(5, 30);
                Console.ReadLine();
                SendDataToDP(5, 40);
                Console.ReadLine();
                SendDataToDP(5, 50);
                Console.ReadLine();
                SendDataToDP(5, 0);
            }
        }

  
        public static async Task RunMQTTClientAsync()
        {
            try
            {
                // Create a new MQTT client.
                var factory = new MqttFactory();
                mqttClient = factory.CreateMqttClient();

                var ClientID = Settings.MQTT_CLIENT_ID.Replace("%rnd%", new Guid().ToString());

                var Credentials = new MqttClientCredentials
                {
                    Username = Settings.MQTT_CRED_USERNAME,
                    Password = Settings.MQTT_CRED_PASSWORD
                };

                var TlsOptions = new MqttClientTlsOptions
                {
                    UseTls = Settings.MQTT_USE_TLS
                };

                var ChannelOptions_WebSocket = new MqttClientWebSocketOptions
                {
                    Uri = Settings.MQTT_SERVER_WEBSOCKET,
                    TlsOptions = TlsOptions
                };

                var ChannelOptions_TCP = new MqttClientTcpOptions
                {
                    Server = Settings.MQTT_SERVER_TCP,
                    TlsOptions = TlsOptions
                };
                
                var clientOptions = new MqttClientOptions();

                if (Settings.MQTT_CONNECTION_MODE == MQTT_CONNECTION_MODE.TCP)
                {
                    clientOptions = new MqttClientOptions
                    {
                        CleanSession = Settings.MQTT_CLEAN_SESSION,
                        ClientId = ClientID,
                        Credentials = Credentials,
                        ChannelOptions = ChannelOptions_TCP
                    };
                }
                else
                {
                    clientOptions = new MqttClientOptions
                    {
                        CleanSession = Settings.MQTT_CLEAN_SESSION,
                        ClientId = ClientID,
                        Credentials = Credentials,
                        ChannelOptions = ChannelOptions_WebSocket
                    };

                }


                mqttClient.ApplicationMessageReceived += async (s, e) =>
                {
                    string[] topics = e.ApplicationMessage.Topic.Split("/");
                    int basePathLevels = Settings.MQTT_BASEPATH.Split("/").Length;
                    if (topics.Length < basePathLevels+1)
                    {
                        Console.WriteLine("Invalid topic path!");
                        return;
                    }
                    switch (topics[basePathLevels])
                    {
                        case "cmd":
                            {
                                try
                                {
                                    SendDataToDP(Convert.ToInt32(topics[basePathLevels + 1]), Convert.ToInt32(Encoding.UTF8.GetString(e.ApplicationMessage.Payload)));
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }

                                break;
                            }
                        case "get":
                            {
                                break;
                            }
                        case "RAW":
                            {
                                //Because we only subscribe to RAW/in, we don't need to check topics[3]. It MUST be "in" to trigger this code.

                                // The HEX data comes as plain text, which is right now stored as a byte array.
                                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload).Replace(" ", ""); //Get the text from the payload and remove the spaces
                                int NumberChars = payload.Length;
                                byte[] payloadAsBytes = new byte[(NumberChars / 2)];
                                for (int i = 0; i < NumberChars; i += 2) //Go through the payload two bytes pr step
                                {
                                    payloadAsBytes[i / 2] = Convert.ToByte(payload.Substring(i, 2), 16); //Convert "two bytes of text" into one byte of "data"
                                }
                                //if (payloadAsBytes[0] != 0x00) { payloadAsBytes = AddZeroAsFirstByte(payloadAsBytes); }
                                //if (Settings.CONNECTION_MODE==CI_CONNECTION_MODE.RS232_MODE) { payloadAsBytes = AddRS232Bytes(payloadAsBytes); }
                                PrintByte(payloadAsBytes, "Sending RAW data");
                                SendThenBlockTransmit(payloadAsBytes); //Send this to the interface for immediate transmit
                                break;
                            }
                        case "config":
                            {
                                switch(topics[basePathLevels + 1])
                                {
                                    case "loadsettings":
                                    case "getsettings":
                                        {
                                            await SendMQTTMessageAsync("BachelorPad/xComfort/config/" + topics[basePathLevels + 1] + "/result", Settings.GetSettingsAsJSON());
                                            break;
                                        }
                                    case "savesettings":
                                    case "setsettings":
                                        {
                                            await SendMQTTMessageAsync("BachelorPad/xComfort/config/" + topics[basePathLevels + 1] + "/result", Settings.WriteSettingsToFile(Encoding.UTF8.GetString(e.ApplicationMessage.Payload)).ToString());
                                            break;
                                        }
                                    default:
                                        {
                                            Console.WriteLine($"Unknown config request: {topics[basePathLevels + 1]}");
                                            break;
                                        }
                                }
                                break;
                            }
                        default:
                            {
                                Console.WriteLine("Unknown topic: " + topics[2]);
                                break;
                            }
                    }
                };


                mqttClient.Connected += async (s, e) =>
                {
                    Console.WriteLine("Connected to MQTT server!");

                    // Subscribe to a topic
                    await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("BachelorPad/xComfort/cmd/#").Build());
                    await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("BachelorPad/xComfort/RAW/in").Build());
                    await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("BachelorPad/xComfort/config/").Build());
                    await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("BachelorPad/xComfort/config/getsettings").Build());
                    await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("BachelorPad/xComfort/config/setsettings").Build());
                    await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("BachelorPad/xComfort/config/loadsettings").Build());
                    await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("BachelorPad/xComfort/config/savesettings").Build());
                    //await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("BachelorPad/xComfort/RAW/out").Build()); // We don't need to subscribe to our own outbound messages...
                    //Console.WriteLine("### SUBSCRIBED ###");
                };

                mqttClient.Disconnected += async (s, e) =>
                {
                    //Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                    await Task.Delay(TimeSpan.FromSeconds(5));

                    try
                    {
                        await mqttClient.ConnectAsync(clientOptions);
                    }
                    catch
                    {
                        Console.WriteLine("Failed to reconnect to MQTT server!");
                    }
                };

                try
                {
                    await mqttClient.ConnectAsync(clientOptions);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Connection failed!" + Environment.NewLine + exception);
                }
                Console.WriteLine("Done connecting to MQTT server...");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //private static IDevice GetDevice(int VID)
        //{
        //    IDevice device = null;
        //    var deviceDefinitionsAll = new List<FilterDeviceDefinition> { new FilterDeviceDefinition { DeviceType = Device.Net.DeviceType.Hid }, new FilterDeviceDefinition { DeviceType = Device.Net.DeviceType.Usb } };

        //    var deviceDefinitions = new List<FilterDeviceDefinition> //vid_188a&pid_1101
        //    {
        //        new FilterDeviceDefinition{ VendorId= Convert.ToUInt32(VID) } //, ProductId=0x1101}
        //        //new FilterDeviceDefinition{ DeviceType= Device.Net.DeviceType.Usb, VendorId= 0x188a, ProductId=0x1101}
        //    };
        //    try
        //    {
        //        int x = 0;
        //        foreach (LibUsbDotNet.Main.UsbRegistry reg in LibUsbDotNet.UsbDevice.AllDevices)
        //        {
        //            Console.WriteLine($"LibUsbDotNet: Processing device {reg.Name} {reg.DevicePath}");
        //            if (reg.Vid == VID)
        //            {
        //                Console.WriteLine($"Step {x++}");
        //                ConnectedDeviceDefinition connectedDeviceDefinition = new ConnectedDeviceDefinition(reg.DevicePath);
        //                Console.WriteLine($"Step {x++}");
        //                DeviceManager deviceManager = DeviceManager.Current;
        //                Console.WriteLine($"Step {x++}");
        //                Console.WriteLine(deviceManager.ToString());
        //                Console.WriteLine($"Step {x++} -----------");

        //                List<IDevice> devices = deviceManager.GetDevicesAsync(deviceDefinitionsAll);
        //                Console.WriteLine($"Step {x++}");

        //                Console.WriteLine(devices.Count);
        //                Console.WriteLine($"Step {x++}");

        //                foreach (IDevice dev in devices)
        //                {
        //                    Console.WriteLine($"xStep {x++}");
        //                    Console.WriteLine(dev.DeviceId);
        //                    if (dev.DeviceId == reg.DevicePath)
        //                    {
        //                        Console.WriteLine($"FOUND IT!");
        //                        device = dev;
        //                    }
        //                }
        //                Console.WriteLine($"Step {x++}");
        //                //Console.WriteLine($"Step {x++}");
        //                //Console.WriteLine(DeviceManager.Current.GetDevice(new ConnectedDeviceDefinition(reg.DevicePath)).DeviceId);
        //                //Console.WriteLine($"Step {x++}");
        //                //IDevice device = DeviceManager.Current.GetDevice(new ConnectedDeviceDefinition(reg.DevicePath));
        //                //Console.WriteLine($"Step {x++}");
        //                //myDevice = DeviceManager.Current.GetDevice(connectedDeviceDefinition);
        //                //Console.WriteLine($"Step {x++}");
        //            }
        //            Console.WriteLine($"yStep {x++}");
        //        }
        //        Console.WriteLine($"zStep {x++}");
        //        //var anIDevice = DeviceManager.Current.GetDevice(new ConnectedDeviceDefinition(myDevice.DevicePath));


        //        //var allUSBdevices = await DeviceManager.Current.GetDevicesAsync(deviceDefinitionsAll);

        //        //Get the first available device and connect to it
        //        //var devices = await DeviceManager.Current.GetDevicesAsync(deviceDefinitions);
        //        //myDevice = devices.FirstOrDefault();
        //        //if (devices.Count==0 || myDevice == null)
        //        //LibUsbDevice myLibDevice = 
        //        //myDevice = DeviceManager.Current.GetDevice(new ConnectedDeviceDefinition("usbdevice1.6"));
        //        //foreach (IDevice dev in allUSBdevices)
        //        //{
        //        //    Console.WriteLine($"Checking USB device {dev.DeviceId}...");

        //        //    var newIDevice = DeviceManager.Current.GetDevice(new ConnectedDeviceDefinition(dev.DeviceId));
        //        //    //LibUsbDotNet.IUsbDevice libDev = 




        //        //    //if (dev. == 0x188a)
        //        //    //{
        //        //    //    Console.WriteLine($"{dev.DevicePath} is a match!");
        //        //    //    myDevice = new LibUsbDevice(dev.UsbRegistryInfo.Device, 2000);
        //        //    //}
        //        //};
        //        if (device == null)
        //        {
        //            Console.ForegroundColor = ConsoleColor.Red;
        //            Console.WriteLine("Unable to find the CI!");
        //            Console.WriteLine("Are you sure it's supposed to be a USB device and not connected via RS232?");
        //            Console.WriteLine("There is no reason to continue, I'm going to self-terminate now...");
        //            Console.ForegroundColor = ConsoleColor.Gray;
        //            Environment.Exit(0);
        //            return null;
        //        }
        //        else
        //        {
        //            Console.WriteLine($"Using device {device.DeviceId}...");
        //            return device;
        //        }
        //    } catch (Exception exception)
        //    {
        //        Console.WriteLine(exception.Message);
        //    }
        //    return null;
        //}

        public static async Task ConnectToHIDAsync()
        {
            Console.WriteLine("Connecting to Communication Interface (CI) using USB...");

#if LIBUSB
            LibUsbUsbDeviceFactory.Register();
#else
            WindowsUsbDeviceFactory.Register();
            WindowsHidDeviceFactory.Register();
#endif
            try
            {
                int VID = 0x188a;   // This is the Vendor ID that we are looking for.
                string desiredDeviceID = LibUsbDotNet.UsbDevice.OpenUsbDevice(new LibUsbDotNet.Main.UsbDeviceFinder(0x188a, 0x1101)).DevicePath; // We don't know this value at compile time.

                //LibUsbDotNet.UsbDevice.OpenUsbDevice(new LibUsbDotNet.Main.UsbDeviceFinder(0x188a, 0x1101)).DevicePath

                //LibUsbDotNet.Main.UsbDeviceFinder usbDeviceFinder = new LibUsbDotNet.Main.UsbDeviceFinder(0x188a, 0x1101);
                //LibUsbDotNet.UsbDevice usbDevice = LibUsbDotNet.UsbDevice.OpenUsbDevice(usbDeviceFinder);
                //Console.WriteLine($"Using device: {usbDevice.Info.ProductString} { usbDevice.DevicePath}");

                //foreach (LibUsbDotNet.Main.UsbRegistry reg in LibUsbDotNet.UsbDevice.AllDevices)
                //{
                //    if (reg.Vid == VID)
                //    {
                //        Console.WriteLine($"LibUsbDotNet: Found device {reg.Name} {reg.DevicePath}");
                //        desiredDeviceID = reg.DevicePath;
                //    }
                //}

#if false
                var deviceDefinitions = new List<FilterDeviceDefinition> //vid_188a&pid_1101
                {
                    new FilterDeviceDefinition{ VendorId= Convert.ToUInt32(VID) } //, ProductId=0x1101}
                    //new FilterDeviceDefinition{ DeviceType= Device.Net.DeviceType.Usb, VendorId= 0x188a, ProductId=0x1101}
                };
                foreach (IDevice dev in await DeviceManager.Current.GetDevicesAsync(deviceDefinitions))
                {
                    if (dev == null) { Console.WriteLine("Skipping null-entry"); continue; }
                    Console.WriteLine($"LibUsbDevice: Checking desired ID up against {dev.DeviceId}...");
                    if (dev.DeviceId == desiredDeviceID)
                    {
                        myDevice = dev;
                    }
                }
#else
                Console.WriteLine($"We desire {desiredDeviceID}");

                DeviceManager deviceManager = DeviceManager.Current;
                Console.WriteLine(deviceManager.ToString());
                uint? PID = 0x1101;
                ushort? UP = 0;
                List<FilterDeviceDefinition> filterDeviceDefinitions = new List<FilterDeviceDefinition>
                {
                    new FilterDeviceDefinition { VendorId = Convert.ToUInt32(VID), DeviceType = Device.Net.DeviceType.Hid, ProductId = PID, UsagePage = UP },
                    new FilterDeviceDefinition { VendorId = Convert.ToUInt32(VID), DeviceType = Device.Net.DeviceType.Usb, ProductId = PID, UsagePage = UP }
                };
                foreach (IDevice dev in await deviceManager.GetDevicesAsync(filterDeviceDefinitions))
                {
                    if (dev.DeviceId == desiredDeviceID)
                    {
                        Console.WriteLine($"Found device: {dev.DeviceId}");
                        myDevice = dev;
                    }
                }
                
#endif
                await myDevice.InitializeAsync();
                readyToTransmit = true;

                Console.WriteLine("Listening for incomming data...");
                bootComplete = true;
                do
                {
                    var readBuffer = await myDevice.ReadAsync();
                    if (readBuffer.Length > 0) { IncommingData(readBuffer); }
                } while (true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public static async void SendThenBlockTransmit(byte[] dataToSend)
        {
            readyToTransmit = false;    // Stop any other thread from sending right now

            // Prepare the packet by adding extra bytes if needed.
            if (Settings.CONNECTION_MODE == CI_CONNECTION_MODE.RS232_MODE) { dataToSend = AddRS232Bytes(dataToSend); }
            if (dataToSend[0] != 0x00 && dataToSend[0] != Settings.RS232_STARTBYTE ) { dataToSend = AddZeroAsFirstByte(dataToSend); }

            Array.Resize(ref dataToSend, myDevice.ConnectedDeviceDefinition.WriteBufferSize.Value); //If we don't fill the buffer, it will repeat the data instead of using 0x00. That causes strangeness...

            DateTime start = DateTime.Now;
            await myDevice.WriteAsync(dataToSend);
            
            while (DateTime.Now.Subtract(start).TotalSeconds < 5)
            {
                if (readyToTransmit) { return; } // No need to wait for timeout!
            }
            Console.WriteLine("Transmit blockage timed out!");
            readyToTransmit = true; // Unlock due to timeout.
        }

        public static async Task SendMQTTMessageAsync(string topic, string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                   .WithTopic(topic)
                   .WithPayload(payload)
                   .WithExactlyOnceQoS()
                   .WithRetainFlag(false)
                   .Build();
            await mqttClient.PublishAsync(message);
        }
               
        public static void openSerialport()
        {
            SerialPort com = new SerialPort(Settings.RS232_PORT,Settings.RS232_BAUD)
            {
                StopBits = StopBits.One,
                Parity = Parity.None
            };

            com.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            
            com.Open();
            Console.WriteLine($"{com.PortName} is open: " + com.IsOpen);

            //{ 0x5A, 0x06, 0xB1, 0x02, 0x0A, 0x01, 0x70, 0xA5 }; // Turns on DP #2

            byte[] myCommand = { 0x5A, 0x04, 0xB2, 0x1B, 0x00, 0xA5 }; // Requests the software versions of the interface 
            PrintByte(myCommand, "Requesting software version");
            com.Write(myCommand, 0, 6);
            
        }

        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            //Console.WriteLine("Receiving data:");
            SerialPort sp = (SerialPort)sender;
            
            string myData = sp.ReadExisting();
            int cmdLength = 0;
            foreach (byte b in myData)
            {
                if (b == Settings.RS232_STARTBYTE) {
                    acceptingData = true;
                    continue;
                }
                if (acceptingData)
                {
                    receivedData.Add(b);
                    //Now we need to know the value of the second byte.
                    if (cmdLength==0 && receivedData.Count > 1) { cmdLength = receivedData[0]; }
                    if (cmdLength>0 && receivedData.Count == (cmdLength-0)){
                        //We are done!
                        acceptingData = false;
                        byte[] dataAsBytes = receivedData.ToArray();
                        dataAsBytes = RemoveRS232Bytes(dataAsBytes);
                        PrintByte(dataAsBytes, "Serial data");
                        IncommingData(dataAsBytes);
                        receivedData.Clear();
                    }
                    //Console.Write(Convert.ToString(b, 16).PadLeft(2, '0') + " ");
                }
            }
        }

        private static async void broadcastChange(int dataPointID, string dataValue){
            //This is where we tell BachelorPad about the change that has been made.
            //(Could also consider making this compatible with OpenHAB2 and other such systems, so that more could benefit from it)
            Console.WriteLine("Datapoint " + dataPointID + " (" + datapoints.Find(x => x.DP == dataPointID).Name + ") just reported value " + dataValue);
            await SendMQTTMessageAsync("BachelorPad/xComfort/" + dataPointID + "/set/", dataValue);
        }

        private static async void broadcastAck(int dataPointID, string dataValue)
        {
            //This is where we tell BachelorPad about the change that has been made.
            //(Could also consider making this compatible with OpenHAB2 and other such systems, so that more could benefit from it)
            Console.WriteLine("Datapoint " + dataPointID + " (" + datapoints.Find(x => x.DP == dataPointID).Name + ") just confirmed value " + dataValue);
            await SendMQTTMessageAsync("BachelorPad/xComfort/" + dataPointID + "/ack/", dataValue);
        }

        private static void ImportDatapointsFromFile(String filePath)
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
               The 
             */
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Datapoint file not found!");
                return;
            }
            string aline;
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            using (StreamReader reader = new StreamReader(fileStream))
            {
                while((aline = reader.ReadLine()) != null)
                {
                    string[] line = aline.Split("\t");
                    datapoints.Add(new Datapoint(Convert.ToInt32(line[0]), line[1], Convert.ToInt32(line[2]), Convert.ToInt32(line[3]), Convert.ToInt32(line[4]), Convert.ToInt32(line[5]), Convert.ToInt32(line[6]), line[7]));
                    //Console.WriteLine("Added datapoint #" + line[0] + " named " + line[1]);
                }
                Console.WriteLine("There are now " + datapoints.Count + " datapoints registered in the system!");
            }
            fileStream.Close();
        }

        private static void ImportDatapointsOneByOne(String dataPointLine)
        {
            //Allows us to add a single datapoint through some other method than reading the file from disk.
            string[] line = dataPointLine.Split("\t");
            datapoints.Add(new Datapoint(Convert.ToInt32(line[0]), line[1], Convert.ToInt32(line[2]), Convert.ToInt32(line[3]), Convert.ToInt32(line[4]), Convert.ToInt32(line[5]), Convert.ToInt32(line[6]), line[7]));
        }

        static async void IncommingData(byte[] dataFromCI) //We've got data from the CI
        {
            if (dataFromCI[0] == 0) { dataFromCI = RemoveFirstByte(dataFromCI); } // CKOZ-00/14 triggers this, but CKOZ-00/03 doesn't...
            PrintByte(dataFromCI, "Incomming data");
            /*
            Example of an acknowledgement message (OK_MRF):
                    Start   Len     Type    St-Type     Status  Seq+Pri     Ack                         Stop
            RS232:  5A      08      C3      1C          04      70          10          00      00      A5
            USB:            08      C3      1C          04      70          10          00      00
                            8 Byte  Status  OK          MRF     7+Std       ACK_DIRECT  NA      NA
            
            Example of an actuator response/status message:
                            Len     Type    DP      Msg T.  Data T.     Info Sh.    Data1   Data2   Data3   Data4   RSSI    Battery
            USB:            0C      C1      02      70      00          01          00      00      00      00      40      10
                            12 Byte Rx      Dp 2    Status  No Data     On                                          Signal  Mains pwr
            */

            if (Settings.RAW_ENABLED)
            {
               await SendMQTTMessageAsync("BachelorPad/xComfort/RAW", FormatByteForPrint(dataFromCI, true));
            }

            byte MGW_TYPE = dataFromCI[1];
            switch (MGW_TYPE){
                case MGW_PT_RX: // Incomming transmission from some device
                    {
                        //                          Length          Type          Datapoint       Msg type      Data type      Info short               {   Data 0          Data 1          Data 2          Data 3   }      RSSI            Battery
                        //HandleRX(new PT_RX.Packet(dataFromCI[0], dataFromCI[1], dataFromCI[2], dataFromCI[3], dataFromCI[4], dataFromCI[5], new byte[4] { dataFromCI[6], dataFromCI[7], dataFromCI[8] , dataFromCI[9]}, dataFromCI[10], dataFromCI[11], 0));
                        HandleRX(new PT_RX.Packet(dataFromCI[0], dataFromCI[1], dataFromCI[2], dataFromCI[3], dataFromCI[4], dataFromCI[5], new byte[4] { dataFromCI[9], dataFromCI[8], dataFromCI[7], dataFromCI[6] }, dataFromCI[10], dataFromCI[11], dataFromCI[12]));
                        break;
                    }
                case MGW_PT_TX: // This is strictly speaking a packet that we are sending, never receiving...
                    {
                        Console.WriteLine("If you're seeing this, it means that outbound data has ended up as inbound. This is not really possible!");
                        break;
                    }
                case MGW_PT_CONFIG: // Configuration info
                    {
                        Console.WriteLine("Config data!");
                        break;
                    }
                case MGW_PT_STATUS: // Incomming status. Generated by the interface device, not arrived by radio transmissions.
                    {
                        //                                Length         Type           StatusType     Status         StatusData {   Data 0          Data 1          Data 2          Data 3   }
                        HandleStatus(new PT_STATUS.Packet(dataFromCI[0], dataFromCI[1], dataFromCI[2], dataFromCI[3], new byte[4]{ dataFromCI[4], dataFromCI[5], dataFromCI[6], dataFromCI[7] }));
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Unexpected type: " + Convert.ToString(MGW_TYPE,16).ToUpper().PadLeft(2,'0'));
                        break;
                    }
            }

            int dataLength = dataFromCI[0];
            byte dataType = dataFromCI[1];

        }

        static async void SendDataToDP(int DP, double dataDouble)
        {
            if (!readyToTransmit)
            {
                // We're not ready, let's wait...
                DateTime start = DateTime.Now;
                while (!readyToTransmit)
                {
                    if (DateTime.Now.Subtract(start).TotalSeconds > 10)
                    {
                        // This should never actually happen, as there is another timeout function.
                        // But we'll include it anyway, as it could prevent a total hang if the first function fails.
                        Console.WriteLine($"Command to DP #{DP} timed out!");
                        readyToTransmit = true;
                        return;
                    }
                }
            };

            Datapoint myDP = datapoints.Find(x => x.DP == DP);
            DeviceType myDT = devicetypes.Find(x => x.ID == myDP.Type);

            byte[] myCommand = new byte[myDevice.ConnectedDeviceDefinition.WriteBufferSize.Value]; 
            myCommand[0] = 0x00; // This one is not interpreted as data, it is ignored.
            myCommand[1] = 0x09; // This is the length of the packet. It can be dynamic, but it's also safe to use a fixed value of 0x09 and pad with 0x00.
            myCommand[2] = 0xB1; // This indicates that we want to control a datapoint.
            myCommand[3] = Convert.ToByte(DP); // The datapoint to control.
            myCommand[4] = 0x00; // What kind of "event" we want the datapoint to perform, such as set mode/state/level
            myCommand[5] = 0x00; // Event data, such as "ON"/"OFF"/"42%"
            myCommand[6] = 0x00; // Sometimes event data requires more than a single byte.
            myCommand[7] = 0x00; // ---- || -----
            myCommand[8] = 0x00; // ---- || -----
            myCommand[9] = 0x00; // Sequence number + priority (If used) 0x00 is a safe value in any case.

            Console.WriteLine($"Setting DP #{ DP } ({ myDP.Name }) to {dataDouble}.");

            switch (myDP.Type)
            {
                case 16: // Switching Actuator
                    {
                        myCommand[4] = PT_TX.MGW_TX_EVENT.MGW_TE_SWITCH;
                        if (dataDouble > 0) { myCommand[5] = PT_TX.MGW_TX_EVENT_DATA.MGW_TED_ON; } //No need for else, because it's already 0x00.
                        break;
                    }
                case 17: // Dimming Actuator
                    {
                        myCommand[4] = PT_TX.MGW_TX_EVENT.MGW_TE_DIM;
                        myCommand[5] = PT_TX.MGW_TX_EVENT_DATA.MGW_TED_PERCENT;
                        myCommand[6] = Convert.ToByte(dataDouble);
                        break;
                    }
                case 18: // Jalousie Actuator
                    {

                        break;
                    }
            }
            
            //Update the sequence counter and history
            int shiftedCounter = sequenceCounter << 4; // This bit shift places the value in the upper nibble, allowing the lower nibble to be used as priority
            myCommand[9] = Convert.ToByte(shiftedCounter);
            messageHistory[sequenceCounter] = myCommand;
            sequenceCounter++;
            if (sequenceCounter > 15) { sequenceCounter = 0; } // Reset to 0 in order to keep the size to 4 bits.

            //Send the data
            PrintByte(myCommand, "\nOutgoing data");
            SendThenBlockTransmit(myCommand);
        }

        static void HandleStatus(PT_STATUS.Packet statusPacket) // Handling packets containing status info
        {
            //Example of the acnknowledge message (OK_MRF)
            //Length  Type  StatusType  Status  StatusData  Ack  N/A  N/A
            //08      C3    1C          04      70          10   00   00
            bool denyReady = false;
            switch (statusPacket.MGW_ST_TYPE)
            {
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_CONNEX:
                    {
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x01: //MGW_CM_AUTO(default)
                                {
                                    Console.WriteLine($"Interface connection mode: AUTO (default)");
                                    break;
                                }
                            case 0x02: //MGW_CM_USB
                                {
                                    Console.WriteLine($"Interface connection mode: USB");
                                    break;
                                }
                            case 0x03: //MGW_CM_RS232
                                {
                                    Console.WriteLine($"Interface connection mode: RS232");
                                    break;
                                }
                            default:
                                {
                                    Console.WriteLine($"Unknown connex status: {statusPacket.MGW_ST_STATUS}");
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_ERROR:
                    {
                        
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x00: //MGW_STS_GENERAL (DATA: specific code)      General Error-Msg
                                {
                                    switch (statusPacket.MGW_ST_DATA[0])
                                    {
                                        case 0xA9:
                                            {
                                                Console.WriteLine("General error!\nAssuming invalid datapoint for this interface. (Consider using MRF to update associations)");
                                                break;
                                            }
                                        default:
                                            {
                                                Console.WriteLine($"Error! General error, data: {Convert.ToString(statusPacket.MGW_ST_DATA[0], 16).ToUpper().PadLeft(2, '0')}, {Convert.ToString(statusPacket.MGW_ST_DATA[1], 16).ToUpper().PadLeft(2, '0')}");
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case 0x01://MGW_STS_UNKNOWN (DATA: specific code)       Msg Unknown
                                {
                                    Console.WriteLine($"Error! Unknown error, data: {Convert.ToString(statusPacket.MGW_ST_DATA[0], 16).ToUpper().PadLeft(2, '0')}, {Convert.ToString(statusPacket.MGW_ST_DATA[1], 16).ToUpper().PadLeft(2, '0')}");
                                    break;
                                }
                            case 0x02://MGW_STS_DP_OOR                              Datapoint out of range
                                {
                                    Console.WriteLine("Error! Datapoint out of range!");
                                    break;
                                }
                            case 0x03://MGW_STS_BUSY_MRF                            RF Busy (Tx Msg lost)
                                {
                                    Console.WriteLine("Error! RF busy, TX message lost");
                                    break;
                                }
                            case 0x04://MGW_STS_BUSY_MRF_RX                         RF Busy (Rx in progress)
                                {
                                    Console.WriteLine("Error! RF busy, RX in progress...");
                                    break;
                                }
                            case 0x05://MGW_STS_TX_MSG_LOST                         Tx-Msg lost, repeat it (buffer full)
                                {
                                    //Console.WriteLine("Error! TX mesage lost, buffer full!");
                                    Console.Write("WARNING! TX message was lost!");
                                    //readyToTransmit = false;
                                    denyReady = true;
                                    byte maskSequence = 0x0F;      // 00001111
                                    byte seq = statusPacket.MGW_ST_DATA[1];
                                    seq &= maskSequence;
                                    Console.WriteLine($" Re-sending message #{seq}...");
                                    myDevice.WriteAsync(messageHistory[seq]);
                                    break;
                                }
                            case 0x06: //MGW_STS_NO_ACK                             RF ≥90: Timeout, no ACK received!
                                {
                                    Console.WriteLine("Timeout, no ACK reveived!");
                                    break;
                                }
                            default:   //                                           Completely undocumented!
                                {
                                    Console.WriteLine("Undocumented error!");
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_LED:
                    {
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x01: //LED standard mode (default)
                                {
                                    Console.WriteLine($"LED is in standard mode.");
                                    break;
                                }
                            case 0x02: //switch green LED to "reverse" fct
                                {
                                    Console.WriteLine($"LED is in reversed mode.");
                                    break;
                                }
                            case 0x03: //switch LEDs completely off
                                {
                                    Console.WriteLine($"LED is turned off.");
                                    break;
                                }
                            default:
                                {
                                    Console.WriteLine($"Unknown LED status: {statusPacket.MGW_ST_STATUS}");
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_LED_DIM:
                    {
                        Console.WriteLine($"LED brightness: {statusPacket.MGW_ST_STATUS}%");
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_OK: //       The most common status type
                    {
                        readyToTransmit = true; //No matter which of the OK-statuses we get, we know it's allright to transmit a new packet
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x04: // MGW_STS_OK_MRF
                                {
                                    switch (statusPacket.MGW_ST_DATA[1])
                                    {
                                        case 0x00:  //MGW_STD_OKMRF_NOINFO          RF Rel. 60
                                            {
                                                Console.WriteLine("MRF OK!");
                                                break;
                                            }
                                        case 0x10://MGW_STD_OKMRF_ACK_DIRECT        RF ≥90: ACK from controlled device
                                            {
                                                Console.WriteLine("MRF OK! (Direct)");
                                                //broadcastAck()
                                                break;
                                            }

                                        case 0x20://MGW_STD_OKMRF_ACK_ROUTED        RF ≥90: ACK from routing device
                                            {
                                                Console.WriteLine("MRF OK! (Routed)");
                                                break;
                                            }

                                        case 0x30://MGW_STD_OKMRF_ACK
                                            {
                                                Console.WriteLine("MRF OK! (ACK)");
                                                break;
                                            }
                                        case 0x40://MGW_STD_OKMRF_ACK_BM            RF ≥91: ACK, device in learnmode
                                            {
                                                Console.WriteLine("MRF OK! (Device in learn mode)");
                                                break;
                                            }

                                        case 0x50://MGW_STD_OKMRF_DPREMOVED         RF ≥90: Basic Mode: DP removed
                                            {
                                                Console.WriteLine("MRF OK! (Basic, DP removed)");
                                                break;
                                            }
                                    }
                                    
                                    break;
                                }
                            case 0x05: // MGW_STS_OK_CONFIG
                                {
                                    Console.WriteLine("Config OK!");
                                    break;
                                }
                            case 0xCE: // MGW_STS_OK_BTF
                                {
                                    Console.WriteLine("BackToFactory OK!");
                                    break;
                                }
                            default:
                                {
                                    Console.WriteLine($"Unknown status data for MGW_STT_OK: {statusPacket.MGW_ST_DATA[0]}");
                                    break;
                                }
                        }                        
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_RELEASE:
                    {
                        Console.WriteLine($"RF-version: {statusPacket.MGW_ST_DATA[0]}.{statusPacket.MGW_ST_DATA[1]}, Firmware: {statusPacket.MGW_ST_DATA[2]}.{statusPacket.MGW_ST_DATA[3]}");
                        readyToTransmit = true;
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_RS232_BAUD:
                    {
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x01: //MGW_CM_BD1200
                                {
                                    Console.WriteLine($"Interface baudrate: 1200");
                                    break;
                                }
                            case 0x02: //MGW_CM_BD2400
                                {
                                    Console.WriteLine($"Interface baudrate: 2400");
                                    break;
                                }
                            case 0x03: //MGW_CM_BD4800
                                {
                                    Console.WriteLine($"Interface baudrate: 4800");
                                    break;
                                }
                            case 0x04: //MGW_CM_BD9600
                                {
                                    Console.WriteLine($"Interface baudrate: 9600");
                                    break;
                                }
                            case 0x05: //MGW_CM_BD14400
                                {
                                    Console.WriteLine($"Interface baudrate: 14400");
                                    break;
                                }
                            case 0x06: //MGW_CM_BD19200
                                {
                                    Console.WriteLine($"Interface baudrate: 19200");
                                    break;
                                }
                            case 0x07: //MGW_CM_BD38400(actually 37.500 Bit / s))
                                {
                                    Console.WriteLine($"Interface baudrate: 37500");
                                    break;
                                }
                            case 0x08: //MGW_CM_BD57600(default)
                                {
                                    Console.WriteLine($"Interface baudrate: 57600 (Default)");
                                    break;
                                }
                            default:
                                {
                                    Console.WriteLine($"Unknown baudrate: {statusPacket.MGW_ST_STATUS}");
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_RS232_CRC:
                    {
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x00: //Not set
                                {
                                    Console.WriteLine($"CRC not in use");
                                    break;
                                }
                            case 0x01: //Set
                                {
                                    Console.WriteLine($"CRC in use");
                                    break;
                                }
                            default: //Unknown
                                {
                                    Console.WriteLine($"Unknown CRC status: {statusPacket.MGW_ST_STATUS}");
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_RS232_FLOW:
                    {
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x00: //Not set
                                {
                                    Console.WriteLine($"Flow control not in use");
                                    break;
                                }
                            case 0x01: //Set
                                {
                                    Console.WriteLine($"Flow control in use");
                                    break;
                                }
                            default: //Unknown
                                {
                                    Console.WriteLine($"Unknown flow control status: {statusPacket.MGW_ST_STATUS}");
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_SEND_CLASS:
                    {
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x00: //Not set
                                {
                                    Console.WriteLine($"Tg-class not in use");
                                    break;
                                }
                            case 0x01: //Set
                                {
                                    Console.WriteLine($"Tg-class in use");
                                    break;
                                }
                            default: //Unknown
                                {
                                    Console.WriteLine($"Unknown Tg-class status: {statusPacket.MGW_ST_STATUS}");
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_SEND_OK_MRF:
                    {
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x00: //Not set
                                {
                                    Console.WriteLine($"OK_MRF not in use");
                                    break;
                                }
                            case 0x01: //Set
                                {
                                    Console.WriteLine($"OK_MRF in use");
                                    break;
                                }
                            default: //Unknown
                                {
                                    Console.WriteLine($"Unknown OK_MRF status: {statusPacket.MGW_ST_STATUS}");
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_SEND_RFSEQNO:
                    {
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x00: // Not set
                                {
                                    Console.WriteLine($"Send RF sequence number not set!");
                                    break;
                                }
                            case 0x01: // Set
                                {
                                    Console.WriteLine($"Send RF sequence number set!");
                                    break;
                                }
                            default:
                                {
                                    Console.WriteLine($"Unknown RF sequence number status: {statusPacket.MGW_ST_STATUS}");
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_SERIAL:
                    {
                        Console.Write($"Serial: { BitConverter.ToInt32(statusPacket.MGW_ST_DATA, 0)}");
                        Array.Reverse(statusPacket.MGW_ST_DATA);
                        Console.WriteLine($" or { BitConverter.ToInt32(statusPacket.MGW_ST_DATA, 0)} ?");

                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_TIMEACCOUNT:
                    {
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x00://MGW_STS_DATA        DATA contains timeaccount in %
                                {
                                    Console.WriteLine($"Timeaccount: {statusPacket.MGW_ST_DATA[0]}%");
                                    break;
                                }
                            case 0x01://MGW_STS_IS_0        no more Tx-msg possible
                                {
                                    Console.WriteLine($"No more transmissions possible!");
                                    break;
                                }
                            case 0x02://MGW_STS_LESS_10     timeaccount fell under 10%
                                {
                                    Console.WriteLine($"Timeaccount: <10% and sinking.");
                                    break;
                                }
                            case 0x03://MGW_STS_MORE_15     timeaccount climbed above 15%
                                {
                                    Console.WriteLine($"Timeaccount: >15% and rising.");
                                    break;
                                }
                            default:
                                {
                                    Console.WriteLine($"Unknown Timeaccount status: {statusPacket.MGW_ST_STATUS}");
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_COUNTER_RX:
                    {
                        Console.WriteLine($"RX counter: {statusPacket.MGW_ST_STATUS}");
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_COUNTER_TX:
                    {
                        Console.WriteLine($"TX counter: {statusPacket.MGW_ST_STATUS}");
                        break;
                    }
                default:
                    {
                        Console.WriteLine($"Unknown status type: {statusPacket.MGW_ST_TYPE }");
                        break;
                    }
            }
            if (!denyReady) { readyToTransmit = true; } // If not, any status would stop the entire program...
        }
      

        static async void HandleRX(PT_RX.Packet rxPacket, bool assignPacket) // Handling packets containing info about other devices
        {
            try
            {
                // What sort of device are we getting data from?
                Datapoint datapoint = datapoints.Find(x => x.DP == rxPacket.MGW_RX_DATAPOINT);
                if (datapoint == null)
                {
                    Console.WriteLine("Datapoint " + rxPacket.MGW_RX_DATAPOINT + " was not found!");
                    return;
                }
                DeviceType devicetype = devicetypes.Find(x => x.Number == datapoint.Type);

                double[] doubleArrayData = new double[2];
                double doubleData = 0;
                //string stringData = "";


                if (assignPacket)
                {
                    datapoint.LatestDataValues = rxPacket;
                    datapoint.LastUpdate = DateTime.Now;
                }

                // And what does the data mean?
                // To be certain that we know what the data means, we might need to know several things.
                //      For room controllers, we need to know what mode it's in.
                //      For dimmers, we only need the percentage from Info Short.

                Console.WriteLine("DataType=" + devicetype.DataTypes[0].ToString());


                if (devicetype.DataTypes[0] == (MGW_RDT_NO_DATA))
                {
                    //We know that we can get all the information we need from the message type.
                    switch (rxPacket.MGW_RX_MSG_TYPE)
                    {
                        case MGW_RMT_ON:
                            {
                                //The device has been turned on!
                                broadcastChange(datapoint.DP, "ON");
                                break;
                            }
                        case MGW_RMT_OFF:
                            {
                                //The device has been turned off!
                                broadcastChange(datapoint.DP, "OFF");
                                break;
                            }
                        case MGW_RMT_SWITCH_ON:
                            {
                                //The device has been turned on!
                                broadcastChange(datapoint.DP, "ON");
                                break;
                            }
                        case MGW_RMT_SWITCH_OFF:
                            {
                                //The device has been turned off!
                                broadcastChange(datapoint.DP, "OFF");
                                break;
                            }
                        case MGW_RMT_UP_PRESSED:
                            {
                                //"Up" is pressed (and held)!
                                break;
                            }
                        case MGW_RMT_UP_RELEASED:
                            {
                                //"Up" is released!
                                break;
                            }
                        case MGW_RMT_DOWN_PRESSED:
                            {
                                //"Down" is pressed (and held)!
                                break;
                            }
                        case MGW_RMT_DOWN_RELEASED:
                            {
                                //"Down" is released!
                                break;
                            }
                        case MGW_RMT_FORCED:
                            {
                                //Fixed value
                                break;
                            }
                        case MGW_RMT_SINGLE_ON:
                            {
                                //Single contact
                                break;
                            }
                        case MGW_RMT_VALUE:
                            {
                                //Analogue value
                                broadcastChange(datapoint.DP, GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleData).ToString());
                                break;
                            }
                        case MGW_RMT_TOO_COLD:
                            {
                                //"Cold" - This means that the temperature is below the set threshold value
                                break;
                            }
                        case MGW_RMT_TOO_WARM:
                            {
                                //"Warm" - This means that the temperature is above the set threshold value
                                break;
                            }
                        case MGW_RMT_STATUS:
                            {
                                //Data about the current status
                                broadcastAck(rxPacket.MGW_RX_DATAPOINT, rxPacket.MGW_RX_INFO_SHORT.ToString());
                                break;
                            }
                        case MGW_RMT_BASIC_MODE:
                            {
                                //Confirmation: Assigned or Removed RF-Device
                                break;
                            }
                        default:
                            {
                                //If any unexpected values should appear, they'll be handled here.
                                break;
                            }
                    }
                }
                else
                {
                    //We need to "go deeper" to get the information we need.

                    //Since there is a different data type, we need to know more.
                    //These types have other data types than NO_DATA:
                    //5 22 23 24 26 28 51 52 53 54 55 62 65 68 69 71 72 74
                    switch (datapoint.Type)
                    {
                        case 5:     // Room controller
                        case 51:    // Room Controller w/ Switch/Humidity CRCA-00/05
                            {
                                switch (datapoint.Channel)
                                {
                                    case 0: //  Channel 0 is temperature. The same on both device models.
                                        {
                                            switch (datapoint.Mode)
                                            {
                                                case 0:
                                                    {
                                                        //Mode 0 (Send switching commands): MGW_RDT_RC_DATA(temperature and wheel; MGW_RX_MSG_TYPE = MGW_RMT_TOO_COLD / MGW_RMT_TOO_WARM)
                                                        double[] data = new double[2];
                                                        data = GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleArrayData);
                                                        broadcastChange(datapoint.DP, ("Temperature: " + data[1] + ", Wheel position: " + data[0]));
                                                        break;
                                                    }
                                                default:
                                                    {
                                                        //Mode 1 (Send temperature value):  MGW_RDT_RC_DATA(temperature and wheel; MGW_RX_MSG_TYPE = MGW_RMT_VALUE)
                                                        double[] data = new double[2];
                                                        data = GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleArrayData);
                                                        broadcastChange(datapoint.DP, ("Temperature: " + data[1] + ", Wheel position: " + data[0]));
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                    case 1: //  Channel 1 is humidity. Only available on the CRCA-00/05
                                        {
                                            switch (datapoint.Mode)
                                            {
                                                case 0:
                                                    {
                                                        //Mode 0 (Send switching commands): MGW_RDT_FLOAT(humidity value in percent; MGW_RX_MSG_TYPE = MGW_RMT_SWITCH_ON / MGW_RMT_SWITCH_OFF)
                                                        double data = new double();
                                                        data = GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleData);
                                                        broadcastChange(datapoint.DP, data.ToString());
                                                        break;
                                                    }
                                                default:
                                                    {
                                                        //Mode 1 (Send humidity value):     MGW_RDT_FLOAT(humidity value in percent; MGW_RX_MSG_TYPE = MGW_RMT_VALUE)
                                                        double data = new double();
                                                        data = GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleData);
                                                        broadcastChange(datapoint.DP, data.ToString());
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                }
                                break;
                            }
                        case 22:    // Home manager
                            {
                                // This one has 99 channels, and it's impossible to act without knowing what device is associated with each channel (which represents datapoints, actually)
                                break;
                            }
                        case 23:    // Temperature Input
                            {
                                switch (datapoint.Mode)
                                {
                                    case 0:
                                        {
                                            //Mode 0 (Send switching commands): MGW_RDT_INT16_1POINT; MGW_RX_MSG_TYPE = MGW_RMT_TOO_COLD / MGW_RMT_TOO_WARM)
                                            double data = new double();
                                            data = GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleData);
                                            broadcastChange(datapoint.DP, data.ToString());
                                            break;
                                        }
                                    default:
                                        {
                                            //Mode 1 (Send temperature value):  MGW_RDT_INT16_1POINT; MGW_RX_MSG_TYPE = MGW_RMT_VALUE)
                                            double data = new double();
                                            data = GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleData);
                                            broadcastChange(datapoint.DP, data.ToString());
                                            break;
                                        }
                                }
                                break;
                            }
                        case 24:    // Analog Input OR PT-1000 temperature reading
                            {
                                if (rxPacket.MGW_RX_DATA_TYPE == MGW_RDT_INT16_1POINT)
                                {
                                    //This is a temperature reading
                                }
                                else
                                {
                                    //This is an analogue reading
                                }
                                switch (datapoint.Channel)
                                {
                                    case 0:
                                        {
                                            switch (datapoint.Mode)
                                            {
                                                case 0:
                                                    {
                                                        // This is an ON/OFF value
                                                        break;
                                                    }
                                                case 1:
                                                    {
                                                        // This is the analogue voltage value
                                                        break;
                                                    }
                                                case 2:
                                                    {
                                                        // This is a percentage value
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                    case 1:
                                        {
                                            switch (datapoint.Mode)
                                            {
                                                case 0:
                                                    {
                                                        // This is an ON/OFF value
                                                        break;
                                                    }
                                                case 1:
                                                    {
                                                        // This is the analogue voltage value
                                                        break;
                                                    }
                                                case 2:
                                                    {
                                                        // This is a percentage value
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                }
                                break;
                            }
                        case 26:    // Room-manager
                            {
                                break;
                            }
                        case 28:    // Communication Interface 
                            {
                                break;
                            }
                        case 72:    // Communication Interface USB
                            {
                                break;
                            }
                        case 53:    // Impulse input
                            {
                                break;
                            }
                        case 54:    // EMS
                            {
                                break;
                            }
                        case 55:    // E-Raditor Actuator
                            {
                                break;
                            }
                        case 62:    // MEP
                            {
                                break;
                            }
                        case 65:    // HRV
                            {
                                break;
                            }
                        case 68:    // Rosetta Sensor
                            {
                                break;
                            }
                        case 69:    // Rosetta Router
                            {
                                break;
                            }
                        case 71:    // Multi Channel Heating Actuator
                            {
                                break;
                            }
                        case 74:    // Switching Actuator New Generation / w Binary input / w EMS
                            {
                                break;
                            }
                        case 52:    // Router(no communication possible, just ignore it)
                            {
                                break;
                            }
                        default:    // Other stuff
                            {
                                break;
                            }
                    }
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        static void HandleRX(PT_RX.Packet rxPacket) 
        {
            // Default for handling a packet is to assign it to the datapoint as well.
            HandleRX(rxPacket, true);
        }

#region "Helpers"

        static int CInt(String strToConvert) // VB.NET syntax, nast habit to get rid of...
        {
            return Convert.ToInt32(strToConvert);
        }

        //These GetDataFromPacket methods are to avoid casting the type afterwards.
        //Now, the method returns the needed data type based on the calling signature.
        //We can retireve data as Double, an array of Double, and string

        static double[] GetDataFromPacket(byte[] mgw_rx_data, byte mgw_rx_data_type, double[] data)
        {
            switch (mgw_rx_data_type)
            {
                case MGW_RDT_RC_DATA: // 4 bytes(only with room controller) : two values, first temperature, then adjustment wheel
                    {
                        double[] values = new double[2];
                        values[0] = BitConverter.ToUInt16(mgw_rx_data, 0);
                        
                        values[1] = BitConverter.ToUInt16(mgw_rx_data, 2);
                        values[1] = values[1] / 10;

                        return values;
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        static double GetDataFromPacket(byte[] mgw_rx_data, byte mgw_rx_data_type, double data)
        {
            switch (mgw_rx_data_type)
            {
                case MGW_RDT_NO_DATA: // No data
                    {
                        return 0;
                    }
                case MGW_RDT_PERCENT: // 1 byte: 0 = 0% ; 255 = 100%
                    {
                        int percentage = mgw_rx_data[0] / 255;
                        double ret = percentage;
                        return ret;
                    }
                case MGW_RDT_UINT8: // 1 byte, integer number unsigned
                    {
                        int value = mgw_rx_data[0];
                        double ret = value;
                        return ret;
                    }
                case MGW_RDT_INT16_1POINT: // 2 bytes, signed with one decimal (0x00FF => 25.5; 0xFFFF => -0.1)
                    {
                        double value = (BitConverter.ToInt16(mgw_rx_data, 2));
                        
                        value = value / 10;
                        return value;
                    }
                case MGW_RDT_FLOAT: // 4 bytes, 32-bit floating-point number(IEEE 754)
                    {
                        float value = BitConverter.ToSingle(mgw_rx_data, 0);
                        double ret = value;
                        return ret;
                    }
                case MGW_RDT_UINT16: // 2 bytes, integer number unsigned
                    {
                        UInt16 value = BitConverter.ToUInt16(mgw_rx_data, 2);
                        double ret = value;
                        return ret;
                    }
                case MGW_RDT_UINT16_1POINT: // 2 bytes, integer unsigned, value x10   (1 digit after point)
                    {
                        double value = BitConverter.ToUInt16(mgw_rx_data, 2);
                        value = value / 10;
                        return value;
                    }
                case MGW_RDT_UINT16_2POINT: // 2 bytes, integer unsigned, value x100   (2 digits after point)
                    {
                        double value = BitConverter.ToUInt16(mgw_rx_data, 2);
                        value = value / 100;
                        return value;
                    }
                case MGW_RDT_UINT16_3POINT: // 2 bytes, integer unsigned, value x1000   (3 digits after point)
                    {
                        double value = BitConverter.ToUInt16(mgw_rx_data, 2);
                        value = value / 1000;
                        return value;
                    }
                case MGW_RDT_UINT32: // 4 bytes, integer number unsigned
                    {
                        return BitConverter.ToUInt32(mgw_rx_data, 0);
                    }
                case MGW_RDT_UINT32_1POINT: // 4 bytes, integer unsigned, value x10   (1 digit after point)
                    {
                        double value = BitConverter.ToUInt32(mgw_rx_data, 0);
                        value = value / 10;
                        return value;
                    }
                case MGW_RDT_UINT32_2POINT: // 4 bytes, integer unsigned, value x100   (2 digits after point)
                    {
                        double value = BitConverter.ToUInt32(mgw_rx_data, 0);
                        value = value / 100;
                        return value;
                    }
                case MGW_RDT_UINT32_3POINT: // 4 bytes, integer unsigned, value x1000   (3 digits after point)
                    {
                        double value = BitConverter.ToUInt32(mgw_rx_data, 0);
                        value = value / 1000;
                        return value;
                    }
            }
            return 0;
        }

        static string GetDataFromPacket(byte[] mgw_rx_data, byte mgw_rx_data_type, string data)
        {
            switch (mgw_rx_data_type)
            {
                case MGW_RDT_NO_DATA: // No data
                    {
                        return null;
                    }
                case MGW_RDT_PERCENT: // 1 byte: 0 = 0% ; 255 = 100%
                    {
                        int percentage = mgw_rx_data[0] / 255;
                        return percentage.ToString() + "%";
                    }
                case MGW_RDT_UINT8: // 1 byte, integer number unsigned
                    {
                        int value = mgw_rx_data[0];
                        return value.ToString();
                    }
                case MGW_RDT_INT16_1POINT: // 2 bytes, signed with one decimal (0x00FF => 25.5; 0xFFFF => -0.1)
                    {
                        double value = BitConverter.ToInt16(mgw_rx_data, 2);
                        value = value / 10;
                        return value.ToString("0.0");
                    }
                case MGW_RDT_FLOAT: // 4 bytes, 32-bit floating-point number(IEEE 754)
                    {
                        return BitConverter.ToSingle(mgw_rx_data, 0).ToString();
                    }
                case MGW_RDT_UINT16: // 2 bytes, integer number unsigned
                    {
                        return BitConverter.ToUInt16(mgw_rx_data, 2).ToString();
                    }
                case MGW_RDT_UINT16_1POINT: // 2 bytes, integer unsigned, value x10   (1 digit after point)
                    {
                        double value = BitConverter.ToUInt16(mgw_rx_data, 2);
                        value = value / 10;
                        return value.ToString("0.0");
                    }
                case MGW_RDT_UINT16_2POINT: // 2 bytes, integer unsigned, value x100   (2 digits after point)
                    {
                        double value = BitConverter.ToUInt16(mgw_rx_data, 2);
                        value = value / 100;
                        return value.ToString("0.00");
                    }
                case MGW_RDT_UINT16_3POINT: // 2 bytes, integer unsigned, value x1000   (3 digits after point)
                    {
                        double value = BitConverter.ToUInt16(mgw_rx_data, 2);
                        value = value / 1000;
                        return value.ToString("0.000");
                    }
                case MGW_RDT_UINT32: // 4 bytes, integer number unsigned
                    {
                        return BitConverter.ToUInt32(mgw_rx_data, 0).ToString();
                    }
                case MGW_RDT_UINT32_1POINT: // 4 bytes, integer unsigned, value x10   (1 digit after point)
                    {
                        double value = BitConverter.ToUInt32(mgw_rx_data, 0);
                        value = value / 10;
                        return value.ToString("0.0");
                    }
                case MGW_RDT_UINT32_2POINT: // 4 bytes, integer unsigned, value x100   (2 digits after point)
                    {
                        double value = BitConverter.ToUInt32(mgw_rx_data, 0);
                        value = value / 100;
                        return value.ToString("0.00");
                    }
                case MGW_RDT_UINT32_3POINT: // 4 bytes, integer unsigned, value x1000   (3 digits after point)
                    {
                        double value = BitConverter.ToUInt32(mgw_rx_data, 0);
                        value = value / 1000;
                        return value.ToString("0.000");
                    }
                case MGW_RDT_RC_DATA: // 4 bytes(only with room controller) : two values, first temperature, then adjustment wheel
                    {
                        double[] values = new double[2];
                        values[0] = BitConverter.ToUInt16(mgw_rx_data, 0);
                        values[0] = values[0] / 10;

                        values[1] = BitConverter.ToUInt16(mgw_rx_data, 2);

                        return values[0].ToString() + ";" + values[1].ToString();
                    }
                case MGW_RDT_TIME: // 4 bytes: hour/minute/second/0; example: 23h 59m 59s: 23 59 59 00 = Hex(17 3B 3B 00)
                    {
                        return (mgw_rx_data[0].ToString() + ":" + mgw_rx_data[1].ToString() + ":" + mgw_rx_data[2].ToString());
                    }
                case MGW_RDT_DATE: // 4 bytes: day / weekday&month / century / year; weekday is placed in the high nibble of 2nd Byte, 0=monday, ... 6=sunday; example: sunday, december 31st 2005: 31 108 20 05 = Hex(1F 6C 14 05)
                    {
                        // We need to separate out the weekday from the month
                        byte maskMonth = 0x0F;      // 00001111
                        byte month = mgw_rx_data[1];
                        month &= maskMonth;

                        //We don't really need this data, as the any modern computer system can get the weekday from a date very easily!
                        //byte weekday = mgw_rx_data[1];
                        //byte maskWeekday = 0xF0;    // 11110000
                        //weekday &= maskWeekday;

                        //Return the data as a ISO 8601 formatted string
                        return mgw_rx_data[2].ToString("00")+ mgw_rx_data[3].ToString("00") + "-" + month.ToString("00") + "-" + mgw_rx_data[0].ToString("00");
                    }
            }
            return "Nothing";
        }
        
        public static void PrintByte(byte[] bytesToPrint, string caption, bool minimalistic) // Used for printing byte arrays as HEX values with spaces between. Makes reading much easier!
        {
            Console.WriteLine($"{caption}: {FormatByteForPrint(bytesToPrint,minimalistic)}");
        }

        public static void PrintByte(byte[] bytesToPrint, string caption) // Shorter signature, defaults to minimalistic printing.
        {
            PrintByte(bytesToPrint, caption, true); // Defaults to true
        }

        public static string FormatByteForPrint(byte[] bytesToPrint, bool minimalistic) // Returns a string where the byte array has been written out in HEX with spaces between each value.
        {
            string formatted = "";
            if (bytesToPrint[0] == 0) { bytesToPrint = WindowsHidDevice.RemoveFirstByte(bytesToPrint); } // Catches the issue with outbound data having an extra 0x00 to start with

            int printLength = bytesToPrint[0];
            if (!minimalistic || printLength > bytesToPrint.Length) { printLength = bytesToPrint.Length; } // If set, we only print the intended data, not the entire buffer that we actually have
             
            for (int i = 0; i < printLength; i++)
            {
                formatted += (Convert.ToString(bytesToPrint[i], 16).ToUpper().PadLeft(2, '0') + " ");
            }
            return formatted;
        }

        public static byte[] RemoveFirstByte(byte[] arrayToFix) //Returns a byte array where the first byte has been removed.
        {
            byte[] result = new byte[arrayToFix.Length - 1];
            Array.Copy(arrayToFix, 1, result, 0, arrayToFix.Length - 1);
            return result;
        }

        public static byte[] AddZeroAsFirstByte(byte[] arrayToFix) //Returns a byte array where an extra 0x00 has been added at the beginning.
        {
            byte[] result = new byte[arrayToFix.Length + 1];
            Array.Copy(arrayToFix, 0, result, 1, arrayToFix.Length);
            return result;
        }

        public static byte[] AddRS232Bytes(byte[] arrayToFix) //Returns an array with the extra start and stop bytes required for RS232 communication.
        {
            byte[] result = new byte[arrayToFix.Length + 2];
            Array.Copy(arrayToFix, 0, result, 1, arrayToFix.Length);
            result[0] = Settings.RS232_STARTBYTE;
            result[result.Length - 1] = Settings.RS232_STOPTBYTE;
            return result;
        }

        public static byte[] RemoveRS232Bytes(byte[] arrayToFix) //Returns a shorter array with the RS232 bytes removed.
        {
            if (arrayToFix[0] == Settings.RS232_STARTBYTE) //Check that the first byte actually IS a RS232 start byte
            {
                byte[] result = new byte[arrayToFix.Length - 2];
                Array.Copy(arrayToFix, 1, result, 0, arrayToFix.Length - 2);
                return result;
            }
            else //The array isn't properly RS232 formatted, best leave it alone...
            {
                return arrayToFix;
            }
        }

#endregion
    }
}
