using System;
using System.Collections.Generic;
using System.Text;

namespace xComfortWingman
{
    public class Basic
    {
        public static List<Device> devices = new List<Device>();


        public class Device // Represents a Datapoint. Datapoints represents xComfort devices.
        {
            public string Name { get; set; }
            public int RSSI { get; set; }
            public int Battery { get; set; }
            public DeviceType DeviceType { get; set; }
            public Datapoint Datapoint { get; set; }
            //public List<Value> Value { get; set; } = new List<Value>();
            public void SetValue(string NewValue)
            {
                NewValue = NewValue.ToLower();
                switch (this.DeviceType.ID){
                    case 6: //Switching Actuator
                        if (NewValue=="on" || NewValue="1" || (NewValue.All(Char.IsDigit) && Convert.ToDouble(NewValue)>0))
                        {

                        } else if (NewValue=="off" || NewValue="0")
                        break;
                    case 7: //Dimming Actuator
                        break;

                    case 8: //Jalousie Actuator
                    case 29: //Jalousie Actuator with Security
                        break;
                 
                    case 19: //Home-Manager
                        break;                   
                    case 27: //Analog Actuator
                        break;
                    case 28: //Room-Manager
                        break;
                    case 30: //Communication Interface
                        break;
                    
                    case 35: //Room Controller w/ Switch/Humidity
                    case 36: //Room Controller w/ Switch/Humidity
                    case 37: //Room Controller w/ Switch/Humidity
                    case 38: //Room Controller w/ Switch/Humidity
                        break;
                   
                    case 60: //Multi Channel Heating Actuator
                    case 61: //Multi Channel Heating Actuator
                    case 62: //Multi Channel Heating Actuator
                        break;

                    case 63: //Communication Interface USB
                        break;

                    case 64: //Switching Actuator New Generation
                    case 65: //Switching Actuator New Generation
                    case 66: //Switching Actuator New Generation
                    case 67: //Switching Actuator New Generation
                    case 68: //Switching Actuator New Generation
                    case 69: //Switching Actuator New Generation
                        break;
                   
                    default: // Device is unknown, not implemented or not accepting input.
                        
                }
            }

            public void ReceiveValue()
            {
                switch (this.DeviceType.ID)
                {
                    case 1: //Push-button (Single)
                        break;
                    case 2: //Push-button (Dual)
                        break;
                    case 3: //Push-button (Quad)
                        break;
                    case 4: //Room Controller ( /w Switch)
                        break;
                    case 5: //Room Controller ( /w Switch)
                        break;
                    case 6: //Switching Actuator
                        break;
                    case 7: //Dimming Actuator
                        break;
                    case 8: //Jalousie Actuator
                        break;
                    case 9: //Binary Input, 230V
                        break;
                    case 10: //Binary Input, 230V
                        break;
                    case 11: //Binary Input, 230V
                        break;
                    case 12: //Binary Input, 230V
                        break;
                    case 13: //Binary Input, 230V
                        break;
                    case 14: //Binary Input, Battery
                        break;
                    case 15: //Binary Input, Battery
                        break;
                    case 16: //Binary Input, Battery
                        break;
                    case 17: //Binary Input, Battery
                        break;
                    case 18: //Remote Control 12 Channel (old design)
                        break;
                    case 19: //Home-Manager
                        break;
                    case 20: //Temperature Input
                        break;
                    case 21: //Temperature Input
                        break;
                    case 22: //Analog Input
                        break;
                    case 23: //Analog Input
                        break;
                    case 24: //Analog Input
                        break;
                    case 25: //Analog Input
                        break;
                    case 26: //Analog Input
                        break;
                    case 27: //Analog Actuator
                        break;
                    case 28: //Room-Manager
                        break;
                    case 29: //Jalousie Actuator with Security
                        break;
                    case 30: //Communication Interface
                        break;
                    case 31: //Motion Detector
                        break;
                    case 32: //Remote Control 2 Channel small
                        break;
                    case 33: //Remote Control 12 Channel
                        break;
                    case 34: //Remote Control 12 Channel w/ display
                        break;
                    case 35: //Room Controller w/ Switch/Humidity
                        break;
                    case 36: //Room Controller w/ Switch/Humidity
                        break;
                    case 37: //Room Controller w/ Switch/Humidity
                        break;
                    case 38: //Room Controller w/ Switch/Humidity
                        break;
                    case 39: //Router (no communication possible, just ignore it)
                        break;
                    case 40: //Impulse Input
                        break;
                    case 41: //EMS
                        break;
                    case 42: //EMS
                        break;
                    case 43: //EMS
                        break;
                    case 44: //EMS
                        break;
                    case 45: //E-Raditor Actuator
                        break;
                    case 46: //Remote Control Alarm Pushbutton
                        break;
                    case 47: //BOSCOS (Bed/Chair Occupancy Sensor)
                        break;
                    case 48: //MEP
                        break;
                    case 49: //MEP
                        break;
                    case 50: //MEP
                        break;
                    case 51: //MEP
                        break;
                    case 52: //MEP
                        break;
                    case 53: //MEP
                        break;
                    case 54: //HRV
                        break;
                    case 55: //Rosetta Sensor
                        break;
                    case 56: //Rosetta Sensor
                        break;
                    case 57: //Rosetta Sensor
                        break;
                    case 58: //Rosetta Sensor
                        break;
                    case 59: //Rosetta Router
                        break;
                    case 60: //Multi Channel Heating Actuator
                        break;
                    case 61: //Multi Channel Heating Actuator
                        break;
                    case 62: //Multi Channel Heating Actuator
                        break;
                    case 63: //Communication Interface USB
                        break;
                    case 64: //Switching Actuator New Generation
                        break;
                    case 65: //Switching Actuator New Generation
                        break;
                    case 66: //Switching Actuator New Generation
                        break;
                    case 67: //Switching Actuator New Generation
                        break;
                    case 68: //Switching Actuator New Generation
                        break;
                    case 69: //Switching Actuator New Generation
                        break;
                    case 70: //Router New Generation
                        break;
                    default:

                }
            }
        }

       /* public class Dimmer : Device
        {
            public int level { get; set; }
        }

        public class Switch : Device
        {
            public bool state { get; set; }
        }

        public class RoomController
        {
            public float Temperature { get; set; }
            public float WheelPosition { get; set; }
            
        }

        public class Button
        {
            
        }
        */
    }
}
