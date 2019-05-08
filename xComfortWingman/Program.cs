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
//using Device.Net;
using System.Diagnostics;
using Hid.Net.Windows;

using System.Text;
using MQTTnet.Protocol;
using System.Linq;
using Usb.Net.Windows;
using Device.Net.LibUsb;
using HidSharp;
using System.Threading;
using HidSharp.Reports;

using static xComfortWingman.Logger;

namespace xComfortWingman
{
    class Program
    {
        private static readonly MQTT mqtt = new MQTT();
        public static readonly Settings Settings = new Settings(true);

        private static bool readyToTransmit = false;
        private static bool bootComplete = false;

        
#if LIBUSB
        public static IDevice myDevice;
        //public static LibUsbDotNet.UsbDevice myDevice;
#else
        public static HidDevice myDevice;
#endif
        public static List<Datapoint> datapoints;
        public static List<DeviceType> devicetypes;
        public static List<byte> receivedData = new List<byte>();
        public static bool acceptingData = false;

        public static byte sequenceCounter = 0x00;
        public static byte[][] messageHistory = new byte[15][];

        static void Main(string[] args)
        {
            datapoints = new List<Datapoint>();
            DeviceTypeList dtl = new DeviceTypeList();
            devicetypes = dtl.ListDeviceTypes();

            Console.ForegroundColor = ConsoleColor.Cyan;
            DoLog("Hi, I'm your xComfort Wingman!");
            //DoLog("I'm here to talk to xComfort for you.");
            //DoLog();
            //DoLog("You talk to me using MQTT, and I'll talk to xComfort by using a Communication Interface (CKOZ-00/03 or CKOZ-00/14)");
            //DoLog("The default topic beginning is 'BacheclorPad/xComfort/', and can be changed in the settings.");
            //DoLog();
            //DoLog("Topics I subscribe to:           \t Topic is used for:");
            //DoLog("\t  BacheclorPad/xComfort/cmd/X/ \t\t Listening for instructions for datapoint X (X is a number)");
            //DoLog("\t  BacheclorPad/xComfort/get/X/ \t\t Requesting an updated status from datapoint X (X is a number)");
            //DoLog("\t* BacheclorPad/xComfort/RAW/in/\t\t Sends the payload as RAW data directly to the interface."); 
            //DoLog();
            //DoLog("Topics I publish to:             \t I publish when:");
            //DoLog("\t  BacheclorPad/xComfort/+/set/ \t\t When a device broadcasts data without receiving an instruction first.");
            //DoLog("\t  BacheclorPad/xComfort/+/ack/ \t\t When confirmation of a completed instruction is received.");
            //DoLog("\t* BacheclorPad/xComfort/RAW/   \t\t Reports all RAW data from the interface as it arrives.");
            //DoLog();
            //DoLog("\t* [RAW]\n\t  [This feature can be enabled or disabled in the settings.]");
            //DoLog("\t  [The data is formatted as a human readable string of HEX values with space between each value.]");
            //DoLog("\t  [Example: 06 1B 01 0A 01 00");
            Console.ForegroundColor = ConsoleColor.Gray;
            //DoLog("Current timeout value: " + Settings.RMF_TIMEOUT);

            DoAllTheStuff();
   
            while (true){
                //Do nothing...
            }
        }

        public async static void DoAllTheStuff()
        {
            //ImportDatapoints();
            ImportDatapointsFromFile("Datenpunkte.txt");

            //Communications
            await mqtt.RunMQTTClientAsync(); //Connecting to MQTT
            if (Settings.CONNECTION_MODE == CI_CONNECTION_MODE.USB_MODE)
            {
                await ConnectToCIasHid(); //Connecting to CI as USB HID
            }
            else
            {
                OpenSerialport(); //Connecting to CI via RS232
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            DoLog("Startup complete!");
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
                DoLog("Press Enter to do some diagnostics!",0);
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


        public static Task ConnectToCIasHid()
        {
            //Console.Write("Connecting to CI...");
            DoLog("Connecting to CI...", false);
            var list = DeviceList.Local;
            //list.Changed += (sender, e) => DoLog("Device list changed."); //We don't need to implement support for hotswap right now... 
            var allHidList = list.GetHidDevices().ToArray();
            foreach (HidSharp.HidDevice dev in allHidList)
            {
                if (dev.VendorID == 0x188A && dev.ProductID == 0x1101)
                {
                    //We have found the CI!
                    myDevice = dev;
                    break;
                }
            }
            if (myDevice != null)
            {
                var reportDescriptor = myDevice.GetReportDescriptor();

                if (myDevice.TryOpen(out HidStream hidStream))
                {
                    DoLog("OK", 3, true, 10);
                    DoLog("Listening for xComfort messages...");
                    hidStream.ReadTimeout = Timeout.Infinite;

                    using (hidStream)
                    {
                        var inputReportBuffer = new byte[myDevice.GetMaxInputReportLength()];

                        // -------------------- RAW -------------------------
                        IAsyncResult ar = null;

                        int startTime = Environment.TickCount;
                        while (true)
                        {
                            if (ar == null)
                            {
                                ar = hidStream.BeginRead(inputReportBuffer, 0, inputReportBuffer.Length, null, null);
                            }

                            if (ar != null)
                            {
                                if (ar.IsCompleted)
                                {
                                    int byteCount = hidStream.EndRead(ar);
                                    ar = null;

                                    if (byteCount > 0)
                                    {
                                        //string hexOfBytes = string.Join(" ", inputReportBuffer.Take(byteCount).Select(b => b.ToString("X2")));
                                        //DoLog("  {0}", hexOfBytes);
                                        //PrintByte(inputReportBuffer, "Received data from CI");
                                        IncommingData(inputReportBuffer);
                                    }
                                }
                                else
                                {
                                    ar.AsyncWaitHandle.WaitOne(500);
                                }
                            }
                            uint elapsedTime = (uint)(Environment.TickCount - startTime);
                            //if (elapsedTime >= 20000) { break; } // Stay open for 20 seconds.
                        }
                        // --------------------------------------------------
                    }
                }
            }
            return null;
        }

 

//        public static async Task ConnectToHIDAsync()
//        {
//            DoLog("Connecting to Communication Interface (CI) using USB...");

//#if LIBUSB
//            LibUsbUsbDeviceFactory.Register();
//#else
//            WindowsUsbDeviceFactory.Register();
//            WindowsHidDeviceFactory.Register();
//#endif
//            try
//            {
//                int VID = 0x188a;   // This is the Vendor ID that we are looking for.
//                string desiredDeviceID = LibUsbDotNet.UsbDevice.OpenUsbDevice(new LibUsbDotNet.Main.UsbDeviceFinder(0x188a, 0x1101)).DevicePath; // We don't know this value at compile time.

//                //LibUsbDotNet.UsbDevice.OpenUsbDevice(new LibUsbDotNet.Main.UsbDeviceFinder(0x188a, 0x1101)).DevicePath

//                //LibUsbDotNet.Main.UsbDeviceFinder usbDeviceFinder = new LibUsbDotNet.Main.UsbDeviceFinder(0x188a, 0x1101);
//                //LibUsbDotNet.UsbDevice usbDevice = LibUsbDotNet.UsbDevice.OpenUsbDevice(usbDeviceFinder);
//                //DoLog($"Using device: {usbDevice.Info.ProductString} { usbDevice.DevicePath}");

//                //foreach (LibUsbDotNet.Main.UsbRegistry reg in LibUsbDotNet.UsbDevice.AllDevices)
//                //{
//                //    if (reg.Vid == VID)
//                //    {
//                //        DoLog($"LibUsbDotNet: Found device {reg.Name} {reg.DevicePath}");
//                //        desiredDeviceID = reg.DevicePath;
//                //    }
//                //}

//#if false
//                var deviceDefinitions = new List<FilterDeviceDefinition> //vid_188a&pid_1101
//                {
//                    new FilterDeviceDefinition{ VendorId= Convert.ToUInt32(VID) } //, ProductId=0x1101}
//                    //new FilterDeviceDefinition{ DeviceType= Device.Net.DeviceType.Usb, VendorId= 0x188a, ProductId=0x1101}
//                };
//                foreach (IDevice dev in await DeviceManager.Current.GetDevicesAsync(deviceDefinitions))
//                {
//                    if (dev == null) { DoLog("Skipping null-entry"); continue; }
//                    DoLog($"LibUsbDevice: Checking desired ID up against {dev.DeviceId}...");
//                    if (dev.DeviceId == desiredDeviceID)
//                    {
//                        myDevice = dev;
//                    }
//                }
//#else
//                DoLog($"We desire {desiredDeviceID}");

//                DeviceManager deviceManager = DeviceManager.Current;
//                DoLog(deviceManager.ToString());
//                uint? PID = 0x1101;
//                ushort? UP = 0;
//                List<FilterDeviceDefinition> filterDeviceDefinitions = new List<FilterDeviceDefinition>
//                {
//                    new FilterDeviceDefinition { VendorId = Convert.ToUInt32(VID), DeviceType = Device.Net.DeviceType.Hid, ProductId = PID, UsagePage = UP },
//                    new FilterDeviceDefinition { VendorId = Convert.ToUInt32(VID), DeviceType = Device.Net.DeviceType.Usb, ProductId = PID, UsagePage = UP }
//                };
//                foreach (IDevice dev in await deviceManager.GetDevicesAsync(filterDeviceDefinitions))
//                {
//                    if (dev.DeviceId == desiredDeviceID)
//                    {
//                        DoLog($"Found device: {dev.DeviceId}");
//                        myDevice = dev;
//                    }
//                }
                
//#endif
//                await myDevice.InitializeAsync();
//                readyToTransmit = true;

//                DoLog("Listening for incomming data...");
//                bootComplete = true;
//                do
//                {
//                    var readBuffer = await myDevice.ReadAsync();
//                    if (readBuffer.Length > 0) { IncommingData(readBuffer); }
//                } while (true);
//            }
//            catch (Exception ex)
//            {
//                DoLog(ex.Message,5);
//                DoLog(ex.StackTrace);
//            }
//        }

        public static async void SendThenBlockTransmit(byte[] dataToSend)
        {
            readyToTransmit = false;    // Stop any other thread from sending right now

            // Prepare the packet by adding extra bytes if needed.
            if (Settings.CONNECTION_MODE == CI_CONNECTION_MODE.RS232_MODE) { dataToSend = AddRS232Bytes(dataToSend); }
            if (dataToSend[0] != 0x00 && dataToSend[0] != Settings.RS232_STARTBYTE ) { dataToSend = AddZeroAsFirstByte(dataToSend); }

            //Array.Resize(ref dataToSend, myDevice.ConnectedDeviceDefinition.WriteBufferSize.Value); //If we don't fill the buffer, it will repeat the data instead of using 0x00. That causes strangeness...

            DateTime start = DateTime.Now;
            //await myDevice.WriteAsync(dataToSend);
            
            while (DateTime.Now.Subtract(start).TotalSeconds < 5)
            {
                if (readyToTransmit) { return; } // No need to wait for timeout!
            }
            DoLog("Transmit blockage timed out!");
            readyToTransmit = true; // Unlock due to timeout.
        }

        
               
        public static void OpenSerialport()
        {
            SerialPort com = new SerialPort(Settings.RS232_PORT,Settings.RS232_BAUD)
            {
                StopBits = StopBits.One,
                Parity = Parity.None
            };

            com.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            
            com.Open();
            DoLog($"{com.PortName} is open: " + com.IsOpen);

            //{ 0x5A, 0x06, 0xB1, 0x02, 0x0A, 0x01, 0x70, 0xA5 }; // Turns on DP #2

            byte[] myCommand = { 0x5A, 0x04, 0xB2, 0x1B, 0x00, 0xA5 }; // Requests the software versions of the interface 
            PrintByte(myCommand, "Requesting software version");
            com.Write(myCommand, 0, 6);
            
        }

        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            //DoLog("Receiving data:");
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

        private static async void BroadcastChange(int dataPointID, string dataValue){
            //This is where we tell BachelorPad about the change that has been made.
            //(Could also consider making this compatible with OpenHAB2 and other such systems, so that more could benefit from it)
            DoLog("Datapoint " + dataPointID + " (" + datapoints.Find(x => x.DP == dataPointID).Name + ") just reported value " + dataValue);
            await mqtt.SendMQTTMessageAsync("BachelorPad/xComfort/" + dataPointID + "/set/", dataValue);
        }

        private static async void BroadcastAck(int dataPointID, string dataValue)
        {
            //This is where we tell BachelorPad about the change that has been made.
            //(Could also consider making this compatible with OpenHAB2 and other such systems, so that more could benefit from it)
            DoLog("Datapoint " + dataPointID + " (" + datapoints.Find(x => x.DP == dataPointID).Name + ") just confirmed value " + dataValue);
            await mqtt.SendMQTTMessageAsync("BachelorPad/xComfort/" + dataPointID + "/ack/", dataValue);
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
                DoLog("Datapoint file not found!");
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
                    //DoLog("Added datapoint #" + line[0] + " named " + line[1]);
                }
                DoLog("There are now " + datapoints.Count + " datapoints registered in the system!");
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
                DoLog("Sending RAW data via MQTT...", 1, false);
                await mqtt.SendMQTTMessageAsync("BachelorPad/xComfort/RAW", FormatByteForPrint(dataFromCI, true));
                DoLog("OK", 1, true, 10);
            }

            byte MGW_TYPE = dataFromCI[1];
            switch (MGW_TYPE){
                case MGW_PT_RX: // Incomming transmission from some device
                    {
                        DoLog("This was a RX packet", 1);
                        //                          Length          Type          Datapoint       Msg type      Data type      Info short               {   Data 0          Data 1          Data 2          Data 3   }      RSSI            Battery
                        //HandleRX(new PT_RX.Packet(dataFromCI[0], dataFromCI[1], dataFromCI[2], dataFromCI[3], dataFromCI[4], dataFromCI[5], new byte[4] { dataFromCI[6], dataFromCI[7], dataFromCI[8] , dataFromCI[9]}, dataFromCI[10], dataFromCI[11], 0));
                        HandleRX(new PT_RX.Packet(dataFromCI[0], dataFromCI[1], dataFromCI[2], dataFromCI[3], dataFromCI[4], dataFromCI[5], new byte[4] { dataFromCI[9], dataFromCI[8], dataFromCI[7], dataFromCI[6] }, dataFromCI[10], dataFromCI[11], dataFromCI[12]),true);
                        break;
                    }
                case MGW_PT_TX: // This is strictly speaking a packet that we are sending, never receiving...
                    {
                        DoLog("If you're seeing this, it means that outbound data has ended up as inbound. This is not really possible!",5);
                        break;
                    }
                case MGW_PT_CONFIG: // Configuration info
                    {
                        DoLog("This was a config packet", 1);
                        DoLog("Config data!");
                        break;
                    }
                case MGW_PT_STATUS: // Incomming status. Generated by the interface device, not arrived by radio transmissions.
                    {
                        DoLog("This was a status packet", 1);
                        //                                Length         Type           StatusType     Status         StatusData {   Data 0          Data 1          Data 2          Data 3   }
                        HandleStatus(new PT_STATUS.Packet(dataFromCI[0], dataFromCI[1], dataFromCI[2], dataFromCI[3], new byte[4]{ dataFromCI[4], dataFromCI[5], dataFromCI[6], dataFromCI[7] }));
                        break;
                    }
                default:
                    {
                        DoLog("Unexpected type: " + Convert.ToString(MGW_TYPE,16).ToUpper().PadLeft(2,'0'),4);
                        break;
                    }
            }

            //int dataLength = dataFromCI[0];
            //byte dataType = dataFromCI[1];

        }

        public static async void SendDataToDP(int DP, double dataDouble)
        {
            if (!readyToTransmit)
            {
                // We're not ready, let's wait...
                DoLog("We're not ready to transmit yet...", 2);
                DateTime start = DateTime.Now;
                while (!readyToTransmit)
                {
                    if (DateTime.Now.Subtract(start).TotalSeconds > 10)
                    {
                        // This should never actually happen, as there is another timeout function.
                        // But we'll include it anyway, as it could prevent a total hang if the first function fails.
                        DoLog($"Command to DP #{DP} timed out!",4);
                        readyToTransmit = true;
                        return;
                    }
                    Thread.Sleep(200);
                }
            };

            Datapoint myDP = datapoints.Find(x => x.DP == DP);
            DeviceType myDT = devicetypes.Find(x => x.ID == myDP.Type);

            byte[] myCommand = new byte[myDevice.GetMaxOutputReportLength()];  //.ConnectedDeviceDefinition.WriteBufferSize.Value]; 
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

            DoLog($"Setting DP #{ DP } ({ myDP.Name }) to {dataDouble}.");

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
                                    DoLog($"Interface connection mode: AUTO (default)");
                                    break;
                                }
                            case 0x02: //MGW_CM_USB
                                {
                                    DoLog($"Interface connection mode: USB");
                                    break;
                                }
                            case 0x03: //MGW_CM_RS232
                                {
                                    DoLog($"Interface connection mode: RS232");
                                    break;
                                }
                            default:
                                {
                                    DoLog($"Unknown connex status: {statusPacket.MGW_ST_STATUS}",5);
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
                                                DoLog("General error!\nAssuming invalid datapoint for this interface. (Consider using MRF to update associations)",4);
                                                break;
                                            }
                                        default:
                                            {
                                                DoLog($"Error! General error, data: {Convert.ToString(statusPacket.MGW_ST_DATA[0], 16).ToUpper().PadLeft(2, '0')}, {Convert.ToString(statusPacket.MGW_ST_DATA[1], 16).ToUpper().PadLeft(2, '0')}",4);
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case 0x01://MGW_STS_UNKNOWN (DATA: specific code)       Msg Unknown
                                {
                                    DoLog($"Error! Unknown error, data: {Convert.ToString(statusPacket.MGW_ST_DATA[0], 16).ToUpper().PadLeft(2, '0')}, {Convert.ToString(statusPacket.MGW_ST_DATA[1], 16).ToUpper().PadLeft(2, '0')}",4);
                                    break;
                                }
                            case 0x02://MGW_STS_DP_OOR                              Datapoint out of range
                                {
                                    DoLog("Error! Datapoint out of range!",4);
                                    break;
                                }
                            case 0x03://MGW_STS_BUSY_MRF                            RF Busy (Tx Msg lost)
                                {
                                    DoLog("Error! RF busy, TX message lost", 4);
                                    break;
                                }
                            case 0x04://MGW_STS_BUSY_MRF_RX                         RF Busy (Rx in progress)
                                {
                                    DoLog("Error! RF busy, RX in progress...", 4);
                                    break;
                                }
                            case 0x05://MGW_STS_TX_MSG_LOST                         Tx-Msg lost, repeat it (buffer full)
                                {
                                    //DoLog("Error! TX mesage lost, buffer full!");
                                    Console.Write("WARNING! TX message was lost!", 4);
                                    //readyToTransmit = false;
                                    denyReady = true;
                                    byte maskSequence = 0x0F;      // 00001111
                                    byte seq = statusPacket.MGW_ST_DATA[1];
                                    seq &= maskSequence;
                                    DoLog($" Re-sending message #{seq}...");
                                    //myDevice.WriteAsync(messageHistory[seq]);
                                    break;
                                }
                            case 0x06: //MGW_STS_NO_ACK                             RF ≥90: Timeout, no ACK received!
                                {
                                    DoLog("Timeout, no ACK reveived!", 4);
                                    break;
                                }
                            default:   //                                           Completely undocumented!
                                {
                                    DoLog("Undocumented error!",4);
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
                                    DoLog($"LED is in standard mode.");
                                    break;
                                }
                            case 0x02: //switch green LED to "reverse" fct
                                {
                                    DoLog($"LED is in reversed mode.");
                                    break;
                                }
                            case 0x03: //switch LEDs completely off
                                {
                                    DoLog($"LED is turned off.");
                                    break;
                                }
                            default:
                                {
                                    DoLog($"Unknown LED status: {statusPacket.MGW_ST_STATUS}",4);
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_LED_DIM:
                    {
                        DoLog($"LED brightness: {statusPacket.MGW_ST_STATUS}%");
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
                                                DoLog("MRF OK!");
                                                break;
                                            }
                                        case 0x10://MGW_STD_OKMRF_ACK_DIRECT        RF ≥90: ACK from controlled device
                                            {
                                                DoLog("MRF OK! (Direct)");
                                                //broadcastAck()
                                                break;
                                            }

                                        case 0x20://MGW_STD_OKMRF_ACK_ROUTED        RF ≥90: ACK from routing device
                                            {
                                                DoLog("MRF OK! (Routed)");
                                                break;
                                            }

                                        case 0x30://MGW_STD_OKMRF_ACK
                                            {
                                                DoLog("MRF OK! (ACK)");
                                                break;
                                            }
                                        case 0x40://MGW_STD_OKMRF_ACK_BM            RF ≥91: ACK, device in learnmode
                                            {
                                                DoLog("MRF OK! (Device in learn mode)");
                                                break;
                                            }

                                        case 0x50://MGW_STD_OKMRF_DPREMOVED         RF ≥90: Basic Mode: DP removed
                                            {
                                                DoLog("MRF OK! (Basic, DP removed)");
                                                break;
                                            }
                                    }
                                    
                                    break;
                                }
                            case 0x05: // MGW_STS_OK_CONFIG
                                {
                                    DoLog("Config OK!");
                                    break;
                                }
                            case 0xCE: // MGW_STS_OK_BTF
                                {
                                    DoLog("BackToFactory OK!");
                                    break;
                                }
                            default:
                                {
                                    DoLog($"Unknown status data for MGW_STT_OK: {statusPacket.MGW_ST_DATA[0]}",4);
                                    break;
                                }
                        }                        
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_RELEASE:
                    {
                        DoLog($"RF-version: {statusPacket.MGW_ST_DATA[0]}.{statusPacket.MGW_ST_DATA[1]}, Firmware: {statusPacket.MGW_ST_DATA[2]}.{statusPacket.MGW_ST_DATA[3]}");
                        readyToTransmit = true;
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_RS232_BAUD:
                    {
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x01: //MGW_CM_BD1200
                                {
                                    DoLog($"Interface baudrate: 1200");
                                    break;
                                }
                            case 0x02: //MGW_CM_BD2400
                                {
                                    DoLog($"Interface baudrate: 2400");
                                    break;
                                }
                            case 0x03: //MGW_CM_BD4800
                                {
                                    DoLog($"Interface baudrate: 4800");
                                    break;
                                }
                            case 0x04: //MGW_CM_BD9600
                                {
                                    DoLog($"Interface baudrate: 9600");
                                    break;
                                }
                            case 0x05: //MGW_CM_BD14400
                                {
                                    DoLog($"Interface baudrate: 14400");
                                    break;
                                }
                            case 0x06: //MGW_CM_BD19200
                                {
                                    DoLog($"Interface baudrate: 19200");
                                    break;
                                }
                            case 0x07: //MGW_CM_BD38400(actually 37.500 Bit / s))
                                {
                                    DoLog($"Interface baudrate: 37500");
                                    break;
                                }
                            case 0x08: //MGW_CM_BD57600(default)
                                {
                                    DoLog($"Interface baudrate: 57600 (Default)");
                                    break;
                                }
                            default:
                                {
                                    DoLog($"Unknown baudrate: {statusPacket.MGW_ST_STATUS}",4);
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
                                    DoLog($"CRC not in use");
                                    break;
                                }
                            case 0x01: //Set
                                {
                                    DoLog($"CRC in use");
                                    break;
                                }
                            default: //Unknown
                                {
                                    DoLog($"Unknown CRC status: {statusPacket.MGW_ST_STATUS}", 4);
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
                                    DoLog($"Flow control not in use");
                                    break;
                                }
                            case 0x01: //Set
                                {
                                    DoLog($"Flow control in use");
                                    break;
                                }
                            default: //Unknown
                                {
                                    DoLog($"Unknown flow control status: {statusPacket.MGW_ST_STATUS}", 4);
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
                                    DoLog($"Tg-class not in use");
                                    break;
                                }
                            case 0x01: //Set
                                {
                                    DoLog($"Tg-class in use");
                                    break;
                                }
                            default: //Unknown
                                {
                                    DoLog($"Unknown Tg-class status: {statusPacket.MGW_ST_STATUS}",4);
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
                                    DoLog($"OK_MRF not in use");
                                    break;
                                }
                            case 0x01: //Set
                                {
                                    DoLog($"OK_MRF in use");
                                    break;
                                }
                            default: //Unknown
                                {
                                    DoLog($"Unknown OK_MRF status: {statusPacket.MGW_ST_STATUS}", 4);
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
                                    DoLog($"Send RF sequence number not set!");
                                    break;
                                }
                            case 0x01: // Set
                                {
                                    DoLog($"Send RF sequence number set!");
                                    break;
                                }
                            default:
                                {
                                    DoLog($"Unknown RF sequence number status: {statusPacket.MGW_ST_STATUS}", 4);
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_SERIAL:
                    {
                        Console.Write($"Serial: { BitConverter.ToInt32(statusPacket.MGW_ST_DATA, 0)}");
                        Array.Reverse(statusPacket.MGW_ST_DATA);
                        DoLog($" or { BitConverter.ToInt32(statusPacket.MGW_ST_DATA, 0)} ?");

                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_TIMEACCOUNT:
                    {
                        switch (statusPacket.MGW_ST_STATUS)
                        {
                            case 0x00://MGW_STS_DATA        DATA contains timeaccount in %
                                {
                                    DoLog($"Timeaccount: {statusPacket.MGW_ST_DATA[0]}%");
                                    break;
                                }
                            case 0x01://MGW_STS_IS_0        no more Tx-msg possible
                                {
                                    DoLog($"No more transmissions possible!");
                                    break;
                                }
                            case 0x02://MGW_STS_LESS_10     timeaccount fell under 10%
                                {
                                    DoLog($"Timeaccount: <10% and sinking.");
                                    break;
                                }
                            case 0x03://MGW_STS_MORE_15     timeaccount climbed above 15%
                                {
                                    DoLog($"Timeaccount: >15% and rising.");
                                    break;
                                }
                            default:
                                {
                                    DoLog($"Unknown Timeaccount status: {statusPacket.MGW_ST_STATUS}", 4);
                                    break;
                                }
                        }
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_COUNTER_RX:
                    {
                        DoLog($"RX counter: {statusPacket.MGW_ST_STATUS}");
                        break;
                    }
                case PT_STATUS.MGW_ST_TYPE.MGW_STT_COUNTER_TX:
                    {
                        DoLog($"TX counter: {statusPacket.MGW_ST_STATUS}");
                        break;
                    }
                default:
                    {
                        DoLog($"Unknown status type: {statusPacket.MGW_ST_TYPE }", 4);
                        break;
                    }
            }
            if (!denyReady) { readyToTransmit = true; } // If not, any status would stop the entire program...
        }


        static void HandleRX(PT_RX.Packet rxPacket, bool assignPacket) // Handling packets containing info about other devices
        {
            try
            {
                // What sort of device are we getting data from?
                Datapoint datapoint = datapoints.Find(x => x.DP == rxPacket.MGW_RX_DATAPOINT);
                if (datapoint == null)
                {
                    DoLog("Datapoint " + rxPacket.MGW_RX_DATAPOINT + " was not found!", 4);
                    return;
                }
                DeviceType devicetype = devicetypes.Find(x => x.Number == datapoint.Type);

                double[] doubleArrayData = new double[2];
                double doubleData = 0;
                //string stringData = "";


                if (assignPacket)
                {
                    DoLog("Updating datapoint...", 0);
                    datapoint.LatestDataValues = rxPacket;
                    datapoint.LastUpdate = DateTime.Now;
                }

                // And what does the data mean?
                // To be certain that we know what the data means, we might need to know several things.
                //      For room controllers, we need to know what mode it's in.
                //      For dimmers, we only need the percentage from Info Short.

                DoLog("DataType=" + devicetype.DataTypes[0].ToString());


                if (devicetype.DataTypes[0] == (MGW_RDT_NO_DATA))
                {
                    //We know that we can get all the information we need from the message type.
                    switch (rxPacket.MGW_RX_MSG_TYPE)
                    {
                        case MGW_RMT_ON:
                            {
                                //The device has been turned on!
                                BroadcastChange(datapoint.DP, "ON");
                                break;
                            }
                        case MGW_RMT_OFF:
                            {
                                //The device has been turned off!
                                BroadcastChange(datapoint.DP, "OFF");
                                break;
                            }
                        case MGW_RMT_SWITCH_ON:
                            {
                                //The device has been turned on!
                                BroadcastChange(datapoint.DP, "ON");
                                break;
                            }
                        case MGW_RMT_SWITCH_OFF:
                            {
                                //The device has been turned off!
                                BroadcastChange(datapoint.DP, "OFF");
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
                                BroadcastChange(datapoint.DP, GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleData).ToString());
                                break;
                            }
                        case MGW_RMT_TOO_COLD:
                            {
                                //"Cold" - This means that the temperature is below the set threshold value
                                DoLog("Too cold!");
                                break;
                            }
                        case MGW_RMT_TOO_WARM:
                            {
                                //"Warm" - This means that the temperature is above the set threshold value
                                DoLog("Too hot!");
                                break;
                            }
                        case MGW_RMT_STATUS:
                            {
                                //Data about the current status
                                BroadcastAck(rxPacket.MGW_RX_DATAPOINT, rxPacket.MGW_RX_INFO_SHORT.ToString());
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
                                DoLog($"Unexpected value: {rxPacket.MGW_RX_MSG_TYPE.ToString()}", 4);
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
                    DoLog($"Datapoint type: {datapoint.Type}", 0);
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
                                                        BroadcastChange(datapoint.DP, ("Temperature: " + data[1] + ", Wheel position: " + data[0]));
                                                        break;
                                                    }
                                                default:
                                                    {
                                                        //Mode 1 (Send temperature value):  MGW_RDT_RC_DATA(temperature and wheel; MGW_RX_MSG_TYPE = MGW_RMT_VALUE)
                                                        double[] data = new double[2];
                                                        data = GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleArrayData);
                                                        BroadcastChange(datapoint.DP, ("Temperature: " + data[1] + ", Wheel position: " + data[0]));
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
                                                        BroadcastChange(datapoint.DP, data.ToString());
                                                        break;
                                                    }
                                                default:
                                                    {
                                                        //Mode 1 (Send humidity value):     MGW_RDT_FLOAT(humidity value in percent; MGW_RX_MSG_TYPE = MGW_RMT_VALUE)
                                                        double data = new double();
                                                        data = GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleData);
                                                        BroadcastChange(datapoint.DP, data.ToString());
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
                                            BroadcastChange(datapoint.DP, data.ToString());
                                            break;
                                        }
                                    default:
                                        {
                                            //Mode 1 (Send temperature value):  MGW_RDT_INT16_1POINT; MGW_RX_MSG_TYPE = MGW_RMT_VALUE)
                                            double data = new double();
                                            data = GetDataFromPacket(rxPacket.MGW_RX_DATA, rxPacket.MGW_RX_DATA_TYPE, doubleData);
                                            BroadcastChange(datapoint.DP, data.ToString());
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
            }
            catch (Exception ex)
            {
                DoLog(ex.Message, 5);
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
            DoLog($"{caption}: {FormatByteForPrint(bytesToPrint,minimalistic)}");
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
