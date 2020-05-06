//using System;
//using System.Collections.Generic;
//using System.Text;
//using xComfortWingman;

//namespace xComfortWingman
//{
//    public class Basic
//    {
//        //public List<Device> devices = new List<Device>();

//        public void PopulateDeviceList()
//        {
//            List<Device> deviceList = new List<Device>();

//            foreach (Datapoint dp in CI.datapoints) {



//            }
//        }

//        /* This is what the Homie data looks like:
//         {
//            "dp":17,
//            "name":"DimKitchen",
//            "type":"DimmingActuator125W",
//            "values":[
//                {"dimlevel":72}
                
//            "rssi:"-56",
//            "battery":4
            
//        }
//        {
//            "dp":47,
//            "name":"ThermoBathroom",
//            "type":"RoomController",
//            "values":[
//                "temperature":27,
//                "wheelposition":-1.4,
//                "rssi:"-56",
//                "battery":4
//                ]
//        }
//             */

//        public class DeviceTest
//        {
//            protected string Name { get; set; }
//            protected int DP { get; set; }
//        }

//        public class DeviceTypeTest : DeviceTest
//        {
//            public string Description { get; set; }
//            public string Note { get; set; }
//            private int myCustomInt { get; set; }
//            public int CustomInt()
//            {
//                return 2;
//            }
//            public int CustomInt(int newInt)
//            {
//                this.myCustomInt=newInt;
//                return newInt;
//            }

//            public DeviceTypeTest(string name, int dP, string description, string note, int customInt)
//            {
//                Name = name;
//                DP = dP;
//                Description = description;
//                Note = note;
//                this.myCustomInt = customInt;
//            }
//        }

//        public class ValueTest
//        {
//            public string Name { get; set; }
//            public float Measurement { get; set; }
//        }

//        public class TempTests : ValueTest
//        {
//            public TempTest(float data)
//            {
//                Name = "Temperature";
//                Measurement = data;
//            }
//        }


//        public class Device // Represents a physical xComfort device, identified by the Datapoint
//        {
//            //public Device(string name, List<Value> values, Datapoint datapoint)
//            //{
//            //    Name = name;
//            //    DataValues = values;
//            //    Datapoint = datapoint;
//            //}


//            public string Name { get; set; }
//            public List<DataType> DataValues { get; set; }
//            public Datapoint Datapoint { get; set; }
//        }
//        //public string getValue(Value value)
//        //{
//        //    return DevType.getValue(value);
//        //}

//        //public string setValue(Value value, string dataValue)
//        //{

//        //}
//        //public void xxxSetValue(string NewValue)
//        //{
//        //    NewValue = NewValue.ToLower();
//        //    switch (this.DeviceType.ID){
//        //        case 6: //Switching Actuator
//        //            if (NewValue=="on" || NewValue="1" || (NewValue.All(Char.IsDigit) && Convert.ToDouble(NewValue)>0))
//        //            {

//        //            } else if (NewValue=="off" || NewValue="0")
//        //            break;
//        //        case 7: //Dimming Actuator
//        //            break;

//        //        case 8: //Jalousie Actuator
//        //        case 29: //Jalousie Actuator with Security
//        //            break;

//        //        case 19: //Home-Manager
//        //            break;                   
//        //        case 27: //Analog Actuator
//        //            break;
//        //        case 28: //Room-Manager
//        //            break;
//        //        case 30: //Communication Interface
//        //            break;

//        //        case 35: //Room Controller w/ Switch/Humidity
//        //        case 36: //Room Controller w/ Switch/Humidity
//        //        case 37: //Room Controller w/ Switch/Humidity
//        //        case 38: //Room Controller w/ Switch/Humidity
//        //            break;

//        //        case 60: //Multi Channel Heating Actuator
//        //        case 61: //Multi Channel Heating Actuator
//        //        case 62: //Multi Channel Heating Actuator
//        //            break;

//        //        case 63: //Communication Interface USB
//        //            break;

//        //        case 64: //Switching Actuator New Generation
//        //        case 65: //Switching Actuator New Generation
//        //        case 66: //Switching Actuator New Generation
//        //        case 67: //Switching Actuator New Generation
//        //        case 68: //Switching Actuator New Generation
//        //        case 69: //Switching Actuator New Generation
//        //            break;

//        //        default: // Device is unknown, not implemented or not accepting input.

//        //    }
//        //}

//        //public void xxxReceiveValue()
//        //{
//        //    switch (this.DeviceType.ID)
//        //    {
//        //        case 1: //Push-button (Single)
//        //            break;
//        //        case 2: //Push-button (Dual)
//        //            break;
//        //        case 3: //Push-button (Quad)
//        //            break;
//        //        case 4: //Room Controller ( /w Switch)
//        //            break;
//        //        case 5: //Room Controller ( /w Switch)
//        //            break;
//        //        case 6: //Switching Actuator
//        //            break;
//        //        case 7: //Dimming Actuator
//        //            break;
//        //        case 8: //Jalousie Actuator
//        //            break;
//        //        case 9: //Binary Input, 230V
//        //            break;
//        //        case 10: //Binary Input, 230V
//        //            break;
//        //        case 11: //Binary Input, 230V
//        //            break;
//        //        case 12: //Binary Input, 230V
//        //            break;
//        //        case 13: //Binary Input, 230V
//        //            break;
//        //        case 14: //Binary Input, Battery
//        //            break;
//        //        case 15: //Binary Input, Battery
//        //            break;
//        //        case 16: //Binary Input, Battery
//        //            break;
//        //        case 17: //Binary Input, Battery
//        //            break;
//        //        case 18: //Remote Control 12 Channel (old design)
//        //            break;
//        //        case 19: //Home-Manager
//        //            break;
//        //        case 20: //Temperature Input
//        //            break;
//        //        case 21: //Temperature Input
//        //            break;
//        //        case 22: //Analog Input
//        //            break;
//        //        case 23: //Analog Input
//        //            break;
//        //        case 24: //Analog Input
//        //            break;
//        //        case 25: //Analog Input
//        //            break;
//        //        case 26: //Analog Input
//        //            break;
//        //        case 27: //Analog Actuator
//        //            break;
//        //        case 28: //Room-Manager
//        //            break;
//        //        case 29: //Jalousie Actuator with Security
//        //            break;
//        //        case 30: //Communication Interface
//        //            break;
//        //        case 31: //Motion Detector
//        //            break;
//        //        case 32: //Remote Control 2 Channel small
//        //            break;
//        //        case 33: //Remote Control 12 Channel
//        //            break;
//        //        case 34: //Remote Control 12 Channel w/ display
//        //            break;
//        //        case 35: //Room Controller w/ Switch/Humidity
//        //            break;
//        //        case 36: //Room Controller w/ Switch/Humidity
//        //            break;
//        //        case 37: //Room Controller w/ Switch/Humidity
//        //            break;
//        //        case 38: //Room Controller w/ Switch/Humidity
//        //            break;
//        //        case 39: //Router (no communication possible, just ignore it)
//        //            break;
//        //        case 40: //Impulse Input
//        //            break;
//        //        case 41: //EMS
//        //            break;
//        //        case 42: //EMS
//        //            break;
//        //        case 43: //EMS
//        //            break;
//        //        case 44: //EMS
//        //            break;
//        //        case 45: //E-Raditor Actuator
//        //            break;
//        //        case 46: //Remote Control Alarm Pushbutton
//        //            break;
//        //        case 47: //BOSCOS (Bed/Chair Occupancy Sensor)
//        //            break;
//        //        case 48: //MEP
//        //            break;
//        //        case 49: //MEP
//        //            break;
//        //        case 50: //MEP
//        //            break;
//        //        case 51: //MEP
//        //            break;
//        //        case 52: //MEP
//        //            break;
//        //        case 53: //MEP
//        //            break;
//        //        case 54: //HRV
//        //            break;
//        //        case 55: //Rosetta Sensor
//        //            break;
//        //        case 56: //Rosetta Sensor
//        //            break;
//        //        case 57: //Rosetta Sensor
//        //            break;
//        //        case 58: //Rosetta Sensor
//        //            break;
//        //        case 59: //Rosetta Router
//        //            break;
//        //        case 60: //Multi Channel Heating Actuator
//        //            break;
//        //        case 61: //Multi Channel Heating Actuator
//        //            break;
//        //        case 62: //Multi Channel Heating Actuator
//        //            break;
//        //        case 63: //Communication Interface USB
//        //            break;
//        //        case 64: //Switching Actuator New Generation
//        //            break;
//        //        case 65: //Switching Actuator New Generation
//        //            break;
//        //        case 66: //Switching Actuator New Generation
//        //            break;
//        //        case 67: //Switching Actuator New Generation
//        //            break;
//        //        case 68: //Switching Actuator New Generation
//        //            break;
//        //        case 69: //Switching Actuator New Generation
//        //            break;
//        //        case 70: //Router New Generation
//        //            break;
//        //        default:

//        //    }
//        //}


//        public class SwitchingActuator : Device
//        {
//            public SwitchingActuator(string name, List<Value> values, Datapoint datapoint)
//            {
//                Name = "Switching actuator";
//                DataValues = new List<DataType.RSSI>
//                {
                    
//                };
//                Datapoint = null;
//                }
//            }

//        public class DType : Basic.Device
//        {
            
        
//            public static DimmingActuator()
//            {
//                Name = "Dimming actuator";
//                DataValues = new List<Value> { DimLevel, RSSI, Battery };
//            }
//            public static JalousieActuator()
//            {
//                Name = "Jalousie actuator";
//                DataValues = new List<Value> { DimLevel, RSSI, Battery };
//            }

//            public static BinaryInput()
//            {
//                Name = "Binary input";
//                DataValues = new List<Value> { BinaryValue, RSSI, Battery };
//            }

//            public static RoomController()
//            {
//                Name = "Room controller";
//                DataValues = new List<Value> { Temperature, WheelPosition, RSSI, Battery };
//            }

//            public static RoomController2()
//            {
//                Name = "Room controller with humidity";
//                DataValues = new List<Value> { Temperature, WheelPosition, Humidity, RSSI, Battery };
//            }

//            public static TemperatureSensor()
//            {
//                Name = "Temperature sensor";
//                DataValues = new List<Value> { Temperature, RSSI, Battery };
//            }
//        }

//        public class Value
//        {

//            public string Name { get; }
//            private bool ValueAsBoolean { get; set; }
//            private string ValueAsString { get; set; }
//            private float ValueAsFloat { get; set; }
//            private int ValueAsInt { get; set; }
//            public void SetValue(Value value, string datavalue)
//            {
//                char decimalCharacter = ((1f / 2f).ToString()[1]); //Quick'n dirty way of handling comma and period as decimal character.
//                datavalue = datavalue.Replace(',', '.').Replace('.', decimalCharacter);
//                switch (value)
//                {
//                    case "b":
//                    case "bool":
//                    case "boolean":
//                        this.ValueAsBoolean = false;
//                        if (datavalue.ToLower() == "true" || datavalue.ToLower() == "on" || datavalue.ToLower() == "1" || datavalue.ToLower() == "yes" || (float.TryParse(datavalue, out float tmpFloat) && tmpFloat > 0)) this.ValueAsBoolean = true;
//                    case "f":
//                    case "float":
//                        this.ValueAsFloat = null;
//                        if (float.TryParse(datavalue, out float tmpValueAsFloat))
//                        {
//                            this.ValueAsFloat = tmpValueAsFloat;
//                        }
//                        break;
//                    case "i":
//                    case "int":
//                        this.ValueAsInt = null;
//                        if (float.TryParse(datavalue, out float tmpFloat))
//                        {
//                            this.ValueAsInt = Convert.ToInt32(tmpFloat.ToString("0"));
//                        }
//                        break;
//                    default:
//                        ValueAsString = datavalue;
//                        break;
//                }
//            }
//            public string GetValue(Value value)
//            {
//                switch (value.DataType)
//                {
//                    case "f":
//                    case "float":
//                        return ValueAsFloat.ToString();
//                    case "i":
//                    case "int":
//                        return ValueAsInt.ToString();
//                    default:
//                        return ValueAsString;
//                }
//            }
//        }

//        public RSSI : Value
//        {
//            public string Name = "RSSI";
//            private string DataType = "i";
//        }

//        public static class Battery : Value
//        {
//            public string Name = "Battery";
//            private string DataType = "i";
//        }

//        public class DimLevel : Value
//        {
//            public string Name = "DimLevel";
//            private string DataType = "i";
//        }

//        public class JalousiePos : Value
//        {
//            public string Name = "JalousiePos";
//            private string DataType = "s";
//        }

//        public class SwitchedOn : Value
//        {
//            public string Name = "SwitchedOn";
//            private string DataType = "b";
//        }
//         public class Temperature : Value
//        {
//            public string Name = "Temperature";
//            private string DataType = "f";
//        }
//         public class WheelPosition : Value
//        {
//            public string Name = "WheelPosition";
//            private string DataType = "f";
//        }
//         public class Humidity : Value
//        {
//            public string Name = "Humidity";
//            private string DataType = "f";
//        }
//         public class BinaryValue : Value
//        {
//            public string Name = "BinaryValue";
//            private string DataType = "b";
//        }



//        public void MakeTypeCollection()
//        {
//            List<Value> defaultlist = new List<Value>();
//            defaultlist.add(new Value("RSSI", "i"));
//            defaultlist.add(new Value("Battery", "i"));

//            List<Value> dimmerlist = new List<Value>();
//            dimmerlist.add(new Value("DimLevel", "i"));

//            List<Value> switchlist = new List<Value>();
//            switchlist.add(new Value("SwitchedOn", "b"));

//            List<Value> dimmerlist = new List<Value>();
//            dimmerlist.add(new Value("DimLevel", ""));


//            dimmerlist.add(new Value("Temperature", ""));

//            DevType devType_pb = new DevType("Push-Button (Single)", 1, list);
//        }

//       /* public class Dimmer : Device
//        {
//            public int level { get; set; }
//        }

//        public class Switch : Device
//        {
//            public bool state { get; set; }
//        }

//        public class RoomController
//        {
//            public float Temperature { get; set; }
//            public float WheelPosition { get; set; }
            
//        }

//        public class Button
//        {
            
//        }
//        */
//    }
//}
