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
using System.Diagnostics;
using System.Linq;
using HidSharp;
using System.Threading;

using static xComfortWingman.MyLogger;

namespace xComfortWingman
{
    class Program
    {
        public static readonly Settings Settings = new Settings(true);
        public static readonly DateTime ApplicationStart = DateTime.Now;

        static void Main(string[] args)
        {
            //Console.ForegroundColor = ConsoleColor.Cyan;
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
            //Console.ForegroundColor = ConsoleColor.Gray;
            //DoLog("Current timeout value: " + Settings.RMF_TIMEOUT);

            CI.ImportDatapointsFromFile("C:\\Misc\\Datenpunkte.txt");
            //CI.ImportDatapointsFromFile("Datenpunkte.txt");

            
            MQTT.RunMQTTClientAsync().Wait();
            //CI.ConnectToCI().Wait();
            MQTT.Testing();

            

            DoLog("Startup complete!",4);

            if (Settings.DEBUGMODE)
            {
                DoLog("Press Enter to do some diagnostics!", 0);
                Console.ReadLine();
                CI.SendData(5, 30).Wait();
                Console.ReadLine();
                CI.SendData(5, 40).Wait();
                Console.ReadLine();
                CI.SendData(5, 50).Wait();
                Console.ReadLine();
                CI.SendData(5, 0).Wait();
            }

            while (true){
                //Do nothing...
            }
        }
    }
}
