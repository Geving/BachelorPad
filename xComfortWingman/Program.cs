using System;
using System.Collections.Generic;
using xComfortWingman.Protocol;
using static xComfortWingman.Protocol.PT_RX.MGW_RX_MSG_TYPE;
using static xComfortWingman.Protocol.PT_RX.MGW_RX_DATA_TYPE;
using static xComfortWingman.Protocol.MGW_TYPE;
using System.IO;

namespace xComfortWingman
{
    class Program
    {
        public static List<Datapoint> datapoints;
        public static List<DeviceType> devicetypes;
        static void Main(string[] args)
        {
            datapoints = new List<Datapoint>();
            devicetypes = new List<DeviceType>();

            Console.WriteLine("Hi, I'm your xComfort Wingman!");
            Console.WriteLine("I'm here to talk to xComfort for you.");

            Console.WriteLine("These are the outputs I can provide:");


            Console.WriteLine("\tStatuses:\tON/OFF");
            Console.WriteLine("\tValues:\t100%, 0%, 42%, 21C, 0.7V, 1, 0");
            Console.WriteLine("\tEvents:\tButton released, Button pressed up, Button up");
            Console.WriteLine("\t");
            Console.WriteLine("\tIf you give me a list of datapoints to monitor, I'll provide specialized feedback for them.");
            Console.WriteLine("\tOtherwise I'll provide a more generic MQTT feedback that you can process at your own leasure.");
            Console.WriteLine("\t");
            Console.WriteLine("\tRaiseEvent(Action, source=dp19, message=Cold, value=17.3"); // Pseudo code!
            Console.WriteLine("\t");
            Console.WriteLine("\t");

            //ImportDatapoints();

            //Fake import for now...
            datapoints.Add(new Datapoint(1, "A button!", 12345, 1, 0, 0, 0, ""));
            datapoints.Add(new Datapoint(2, "A double button!", 98765, 2, 0, 0, 0, ""));

        }

        private static void broadcastChange(int dataPointID, string dataValue){
            //This is where we tell BachelorPad about the change that has been made.
            //(Could also consider making this compatible with OpenHAB2 and other such systems, so that more could benefit from it)
            Console.WriteLine("Datapoint " + dataPointID + " just changed value to " + dataValue);
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

            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string[] line = reader.ReadLine().Split("\t");
                datapoints.Add(new Datapoint(Convert.ToInt32(line[0]), line[1], Convert.ToInt32(line[2]), Convert.ToInt32(line[3]), Convert.ToInt32(line[4]), Convert.ToInt32(line[5]), Convert.ToInt32(line[6]), line[7]));
            }
            fileStream.Close();
           
        }

        private static void ImportDatapointsOneByOne(String dataPointLine)
        {
            //Allows us to add a single datapoint through some other method than reading the file from disk.
            string[] line = dataPointLine.Split("\t");
            datapoints.Add(new Datapoint(Convert.ToInt32(line[0]), line[1], Convert.ToInt32(line[2]), Convert.ToInt32(line[3]), Convert.ToInt32(line[4]), Convert.ToInt32(line[5]), Convert.ToInt32(line[6]), line[7]));
        }

        void IncommingData(byte[] dataFromCI)
        {
            //We've got data from the CI
            //First, we split it up!

            /*
            Example of an acknowledgement message (OK_MRF):
                    Start   Len     Type    St-Type     Status  Seq+Pri     Ack                         Stop
            RS232:  5A      08      C3      1C          04      70          10          00      00      A5
            USB:            08      C3      1C          04      70          10          00      00
                            8 Byte  Status  OK          MRF     7+Std       ACK_DIRECT  NA      NA
             */
            /*
            Example of an actuator response/status message:
                            Len     Type    DP      Msg T.  Data T.     Info Sh.    Data1   Data2   Data3   Data4   RSSI    Battery
            USB:            0C      C1      02      70      00          01          00      00      00      00      40      10
                            12 Byte Rx      Dp 2    Status  No Data     On                                          Signal  Mains pwr

            */

            byte MGW_TYPE = dataFromCI[1];
            switch (MGW_TYPE){
                case MGW_PT_RX:
                    {
                        //                          Length          Type          Datapoint       Msg type      Data type      Info short               {   Data 0          Data 1          Data 2          Data 3   }      RSSI            Battery
                        HandleRX(new PT_RX.Packet(dataFromCI[0], dataFromCI[1], dataFromCI[2], dataFromCI[3], dataFromCI[4], dataFromCI[5], new byte[4] { dataFromCI[6], dataFromCI[7], dataFromCI[8] , dataFromCI[9]}, dataFromCI[10], dataFromCI[11], 0));
                        break;
                    }
                case MGW_PT_TX:
                    {
                        break;
                    }
                case MGW_PT_CONFIG:
                    {
                        break;
                    }
                case MGW_PT_STATUS:
                    {
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            int dataLength = dataFromCI[0];
            byte dataType = dataFromCI[1];

        }

        void HandleRX(PT_RX.Packet rxPacket)
        {
            // Default for handling a packet is to assign it to the datapoint as well.
            HandleRX(rxPacket, true);
        }

        void HandleRX(PT_RX.Packet rxPacket, bool assignPacket)
        {           
            // What sort of device are we getting data from?
            Datapoint datapoint = datapoints.Find(x => x.Type == rxPacket.MGW_RX_DATAPOINT);
            DeviceType devicetype = devicetypes.Find(x => x.Number == datapoint.Type);

            double[] doubleArrayData = new double[2];
            double doubleData = 0;
            string stringData = "";


            if (assignPacket) {
                datapoint.LatestDataValues = rxPacket;
                datapoint.LastUpdate = DateTime.Now;
            }

            // And what does the data mean?
            // To be certain that we know what the data means, we might need to know several things.
            //      For room controllers, we need to know what mode it's in.
            //      For dimmers, we only need the percentage from Info Short.


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
                            break;
                        }
                    case MGW_RMT_BASIC_MODE:
                        {
                            //Confirmation: Assigned or Removed RF-Device
                            break;
                        }
                    default:
                        { break; }
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
                                                    break;
                                                }
                                            default:
                                                {
                                                    //Mode 1 (Send temperature value):  MGW_RDT_RC_DATA(temperature and wheel; MGW_RX_MSG_TYPE = MGW_RMT_VALUE)
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
                                                    break;
                                                }
                                            default:
                                                {
                                                    //Mode 1 (Send humidity value):     MGW_RDT_FLOAT(humidity value in percent; MGW_RX_MSG_TYPE = MGW_RMT_VALUE)
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                            }
                            break;
                        }
                    case 23:    // Temperature Input
                        {
                            switch (datapoint.Mode)
                            {
                                case 0:
                                    {
                                        //Mode 0 (Send switching commands): MGW_RDT_INT16_1POINT; MGW_RX_MSG_TYPE = MGW_RMT_TOO_COLD / MGW_RMT_TOO_WARM)
                                        break;
                                    }
                                default:
                                    {
                                        //Mode 1 (Send temperature value):  MGW_RDT_INT16_1POINT; MGW_RX_MSG_TYPE = MGW_RMT_VALUE)
                                        broadcastChange(datapoint.DP, rxPacket.MGW_RX_DATA[0].ToString());
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
                    default:    // Other stuff
                        {
                            break;
                        }
                }
            }
        }

        #region "Helpers"
        static int CInt(String strToConvert)
        {
            return Convert.ToInt32(strToConvert);
        }

        static double[] GetDataFromPacket(byte[] mgw_rx_data, byte mgw_rx_data_type, double[] data)
        {
            switch (mgw_rx_data_type)
            {
                case MGW_RDT_RC_DATA: // 4 bytes(only with room controller) : two values, first temperature, then adjustment wheel
                    {
                        double[] values = new double[2];
                        values[0] = BitConverter.ToUInt16(mgw_rx_data, 0);
                        values[0] = values[0] / 10;

                        values[1] = BitConverter.ToUInt16(mgw_rx_data, 2);

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
                        double value = BitConverter.ToInt16(mgw_rx_data, 2);
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
        
        private static object _GetDataFromPacket(byte[] mgw_rx_data,byte mgw_rx_data_type)
        {
            switch (mgw_rx_data_type){
                case MGW_RDT_NO_DATA: // No data
                    {
                        return null;
                    }
                case MGW_RDT_PERCENT: // 1 byte: 0 = 0% ; 255 = 100%
                    {
                        int percentage = mgw_rx_data[0] / 255;
                        return percentage;
                    }
                case MGW_RDT_UINT8: // 1 byte, integer number unsigned
                    {
                        int value = mgw_rx_data[0];
                        return value;
                    }
                case MGW_RDT_INT16_1POINT: // 2 bytes, signed with one decimal (0x00FF => 25.5; 0xFFFF => -0.1)
                    {
                        double value = BitConverter.ToInt16(mgw_rx_data, 2);
                        value = value / 10;
                        return value;
                    }
                case MGW_RDT_FLOAT: // 4 bytes, 32-bit floating-point number(IEEE 754)
                    {
                        return BitConverter.ToSingle(mgw_rx_data, 0);
                    }
                case MGW_RDT_UINT16: // 2 bytes, integer number unsigned
                    {
                        return BitConverter.ToUInt16(mgw_rx_data, 2);
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
                case MGW_RDT_RC_DATA: // 4 bytes(only with room controller) : two values, first temperature, then adjustment wheel
                    {
                        double[] values = new double[2];
                        values[0] = BitConverter.ToUInt16(mgw_rx_data, 0);
                        values[0] = values[0] / 10;

                        values[1] = BitConverter.ToUInt16(mgw_rx_data, 2);
                        
                        return values;
                    }
                case MGW_RDT_TIME: // 4 bytes: hour/minute/second/0; example: 23h 59m 59s: 23 59 59 00 = Hex(17 3B 3B 00)
                    {
                        return ("{0}:{1}:{2}", mgw_rx_data[0], mgw_rx_data[1], mgw_rx_data[2]);
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
                        return ("{0}{1}-{1}-{2}", mgw_rx_data[2], mgw_rx_data[3], month, mgw_rx_data[0]);
                    }
                case MGW_RDT_ROSETTA: // 4 bytes
                    {
                        return mgw_rx_data;
                    }
                case MGW_RDT_HRV_OUT: // 4 bytes
                    {
                        return mgw_rx_data;
                    }
            }
            return null;
        }
        #endregion
    }
}
