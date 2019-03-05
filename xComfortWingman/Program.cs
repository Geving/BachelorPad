using System;
using System.Collections.Generic;
using xComfortWingman.Protocol;
using static xComfortWingman.Protocol.PT_RX.MGW_RX_MSG_TYPE;
using static xComfortWingman.Protocol.PT_RX.MGW_RX_DATA_TYPE;
using static xComfortWingman.Protocol.MGW_TYPE;


namespace xComfortWingman
{
    class Program
    {
        public static List<Datapoint> datapoints;
        public static List<DeviceType> devicetypes;
        static void Main(string[] args)
        {
            Console.WriteLine("Hi, I'm your xComfort Wingman!");
            Console.WriteLine("I'm here to talk to xComfort for you.");
            ImportDatapoints();

        }

        private static void ImportDatapoints()
        {
            datapoints.Add(new Datapoint(1,"A button!",12345,1,0,0,0,""));
            datapoints.Add(new Datapoint(2, "A double button!", 98765, 2, 0, 0, 0, ""));
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
            //What sort of device are we getting data from?
            Datapoint datapoint = datapoints.Find(x => x.Type == rxPacket.MGW_RX_DATAPOINT);
            DeviceType devicetype = devicetypes.Find(x => x.Number == datapoint.Type);

            //And what does the data mean?
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
                            break;
                        }
                    case MGW_RMT_OFF:
                        {
                            //The device has been turned off!
                            break;
                        }
                    case MGW_RMT_SWITCH_ON:
                        {
                            //The device has been turned on!
                            break;
                        }
                    case MGW_RMT_SWITCH_OFF:
                        {
                            //The device has been turned off!
                            break;
                        }
                    case MGW_RMT_UP_PRESSED:
                        {
                            //"Up" is pressed!
                            break;
                        }
                    case MGW_RMT_UP_RELEASED:
                        {
                            //"Up" is released!
                            break;
                        }
                    case MGW_RMT_DOWN_PRESSED:
                        {
                            //"Down" is pressed!
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
                            //"Cold"
                            break;
                        }
                    case MGW_RMT_TOO_WARM:
                        {
                            //"Warm"
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
                            case 0: //  Channel 0 is temperature. The same on both devices.
                                {
                                    if (datapoint.Mode == 0)
                                    {
                                        //Mode 0 (Send switching commands): MGW_RDT_RC_DATA(temperature and wheel; MGW_RX_MSG_TYPE = MGW_RMT_TOO_COLD / MGW_RMT_TOO_WARM)
                                    }
                                    else
                                    {
                                        //Mode 1 (Send temperature value):  MGW_RDT_RC_DATA(temperature and wheel; MGW_RX_MSG_TYPE = MGW_RMT_VALUE)
                                    }
                                    break;
                                }
                            case 1: //  Channel 1 is humidity. Only available on the CRCA-00/05
                                {
                                    if (datapoint.Mode == 0)
                                    {
                                        //Mode 0 (Send switching commands): MGW_RDT_FLOAT(humidity value in percent; MGW_RX_MSG_TYPE = MGW_RMT_SWITCH_ON / MGW_RMT_SWITCH_OFF)
                                    }
                                    else
                                    {
                                        //Mode 1 (Send humidity value):     MGW_RDT_FLOAT(humidity value in percent; MGW_RX_MSG_TYPE = MGW_RMT_VALUE)
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                case 23:    // Temperature Input
                    {
                        if (datapoint.Mode == 0)
                        {
                            //Mode 0 (Send switching commands): MGW_RDT_INT16_1POINT; MGW_RX_MSG_TYPE = MGW_RMT_TOO_COLD / MGW_RMT_TOO_WARM)
                        }
                        else
                        {
                            //Mode 1 (Send temperature value):  MGW_RDT_INT16_1POINT; MGW_RX_MSG_TYPE = MGW_RMT_VALUE)
                        }
                        break;
                    }
                case 22:    // Home manager
                    {
                        break;
                    }
                case 24:    // Analog Input
                    {
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
}
