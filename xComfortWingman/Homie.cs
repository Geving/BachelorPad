using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xComfortWingman
{
    public class Homie
    {

        public static List<Homie.Node> HomieNodes = new List<Homie.Node>();
        public static List<Homie.Property> HomieProperties = new List<Homie.Property>();
        public static List<Homie.ArrayElement> HomieArrayElements = new List<Homie.ArrayElement>();

        public static ArrayElement GetArrayElement(int DP)
        {
            return HomieArrayElements.Find(x => x.BelongsToDP == DP);
        }

        public static void UpdateArrayElement(int DP,string dataValue)
        {
            UpdateArrayElement(GetArrayElement(DP),dataValue);
        }
        public static void UpdateArrayElement(ArrayElement arrayElement, string dataValue)
        {
            arrayElement.Value = dataValue;
        }

        //public static Dictionary<string, string> PropertyDetails(string propertyName, string baseTopic)
        public static List<PublishModel> PropertyDetails(string propertyName, string baseTopic)
        {
            //Dictionary<string, string> props = new Dictionary<string, string>();
            List<PublishModel> props = new List<PublishModel>();
            props.Add(new PublishModel(baseTopic + propertyName + "/$name", propertyName));                   //homie/super-car/lights/intensity/$name → "Intensity"
            //homie/super-car/lights/intensity/$settable → "true"
            //homie/super-car/lights/intensity/$unit → "%"
            //homie/super-car/lights/intensity/$datatype → "integer"
            //homie/super-car/lights/intensity/$format → "0:100"
            string switcher = propertyName;
            propertyName = "";
            switch (switcher)
            {
                case "power":
                    {
                        props.Add(new PublishModel(baseTopic + propertyName + "/$settable", "true"));
                        props.Add(new PublishModel(baseTopic + propertyName + "/$datatype", "boolean"));
                        break;
                    }
                case "intensity":
                    {
                        props.Add(new PublishModel(baseTopic + propertyName + "/$settable", "true"));
                        props.Add(new PublishModel(baseTopic + propertyName + "/$unit", "%"));
                        props.Add(new PublishModel(baseTopic + propertyName + "/$datatype", "integer"));
                        props.Add(new PublishModel(baseTopic + propertyName + "/$format", "0-100"));
                        break;
                    }
                case "jalousie":
                    {
                        props.Add(new PublishModel(baseTopic + propertyName + "/$settable", "true"));
                        props.Add(new PublishModel(baseTopic + propertyName + "/$datatype", "string"));
                        break;
                    }
                case "buttonstate":
                    {
                        props.Add(new PublishModel(baseTopic + propertyName + "/$settable", "false"));
                        props.Add(new PublishModel(baseTopic + propertyName + "/$datatype", "string"));
                        break;
                    }
                case "wheelposition":
                    {
                        props.Add(new PublishModel(baseTopic + propertyName + "/$settable", "false"));
                        props.Add(new PublishModel(baseTopic + propertyName + "/$datatype", "float"));
                        props.Add(new PublishModel(baseTopic + propertyName + "/$format", "-15:3"));
                        break;
                    }
                case "temperature":
                    {
                        props.Add(new PublishModel(baseTopic + propertyName + "/$settable", "false"));
                        props.Add(new PublishModel(baseTopic + propertyName + "/$unit", "°C"));
                        props.Add(new PublishModel(baseTopic + propertyName + "/$datatype", "float"));
                        props.Add(new PublishModel(baseTopic + propertyName + "/$format", "-50:180"));
                        break;
                    }
                case "humidity":
                    {
                        props.Add(new PublishModel(baseTopic + propertyName + "/$settable", "false"));
                        props.Add(new PublishModel(baseTopic + propertyName + "/$unit", "%"));
                        props.Add(new PublishModel(baseTopic + propertyName + "/$datatype", "integer"));
                        props.Add(new PublishModel(baseTopic + propertyName + "/$format", "0-100"));
                        break;
                    }
                case "battery":
                    {
                        props.Add(new PublishModel(baseTopic + propertyName + "/$settable", "false"));
                        props.Add(new PublishModel(baseTopic + propertyName + "/$datatype", "integer"));
                        props.Add(new PublishModel(baseTopic + propertyName + "/$format", "0-10"));
                        break;
                    }
                case "signal":
                    {
                        props.Add(new PublishModel(baseTopic + propertyName + "/$settable", "false"));
                        props.Add(new PublishModel(baseTopic + propertyName + "/$unit", "-dBm"));
                        props.Add(new PublishModel(baseTopic + propertyName + "/$datatype", "integer"));
                        props.Add(new PublishModel(baseTopic + propertyName + "/$format", "0-120"));
                        break;
                    }
                default:
                    {
                        // This is the bare minimum required to move the data through the system so that it'll be
                        // possible to handle those special devices in the next system...
                        props.Add(new PublishModel(baseTopic + propertyName + "/$settable", "false"));
                        props.Add(new PublishModel(baseTopic + propertyName + "/$datatype", "string"));
                        break;
                    }
            }
            return props;
        }

        public static Property GetHomiePropertyFor(string propertyName, string baseTopic)
        {
            List<PublishModel> props = new List<PublishModel>();
            //homie/super-car/lights/intensity/$settable → "true"
            //homie/super-car/lights/intensity/$unit → "%"
            //homie/super-car/lights/intensity/$datatype → "integer"
            //homie/super-car/lights/intensity/$format → "0:100"
            switch (propertyName)
            {
                case "power":
                    {
                        return new Property { Name = propertyName, Settable = "true", DataType = "boolean", PublishPath = baseTopic };
                    }
                case "intensity":
                    {
                        return new Property { Name = propertyName, Settable = "true", Unit="%" ,DataType = "boolean", Format="0-100", PublishPath = baseTopic };
                    }
                case "jalousie":
                    {
                        return new Property { Name = propertyName, Settable = "true", DataType = "string", PublishPath = baseTopic };
                    }
                case "buttonstate":
                    {
                        return new Property { Name = propertyName, Settable = "false", DataType = "string", PublishPath = baseTopic };
                    }
                case "wheelposition":
                    {
                        return new Property { Name = propertyName, Settable = "false", DataType = "float", Format= "-15:3", PublishPath = baseTopic };
                    }
                case "temperature":
                    {
                        return new Property { Name = propertyName, Settable = "true", DataType = "boolean", PublishPath = baseTopic };
                    }
                case "humidity":
                    {
                        return new Property { Name = propertyName, Settable = "false",Unit="%", DataType = "integer", Format="0-100",  PublishPath = baseTopic };
                    }
                case "battery":
                    {
                        return new Property { Name = propertyName, Settable = "false", DataType = "integer", Format = "0-10", PublishPath = baseTopic };
                    }
                case "signal":
                    {
                        return new Property { Name = propertyName, Settable = "false", Unit = "-dBm", DataType = "integer", Format = "0-120", PublishPath = baseTopic };
                    }
                default:
                    {
                        // This is the bare minimum required to move the data through the system so that it'll be
                        // possible to handle those special devices in the next system...
                        return new Property { Name = propertyName, Settable = "false", DataType = "string", PublishPath = baseTopic };
                    }
            }
        }

        public static string GetPropertiesForDeviceType(DeviceType deviceType)
        {
            // Some devices are really easy, get those out of the way first...
            switch (deviceType.Number)
            {
                // Output-devices
                case 46:
                case 16: { return "power,signal"; }
                case 17: { return "intensity,signal"; }
                case 27:
                case 18: { return "jalousie,signal"; }

                // Input-devices
                case 1: case 2: case 3:             // Push buttons
                case 19: case 20:                   // Binary inputs
                case 21: case 48: case 49:          // Remote controls
                case 50: case 56:                   // ---- || ----     
                    { return "buttonstate,battery,signal"; }
                case 5: { return "temperature,wheelposition,battery,signal"; }
                case 51: { return "temperature,wheelposition,humidity,battery,signal"; }
                case 22: { return "power,signal"; }
            }

            // For the rest, return the name of their data and message type...
            string tmp = "battery,signal,";
            foreach (byte dataType in deviceType.DataTypes)
            {
                if (dataType == Protocol.PT_RX.MGW_RX_DATA_TYPE.MGW_RDT_NO_DATA)
                {
                    foreach (byte msgType in deviceType.MessageTypes)
                    {
                        tmp += (Protocol.PT_RX.MGW_RX_MSG_TYPE.GetTechnicalNameFromByte(msgType)) + ",";
                    }
                }
                else
                {
                    tmp += (Protocol.PT_RX.MGW_RX_DATA_TYPE.GetNameFromByte(dataType)) + ",";
                }
            }
            return tmp;
        }

        public static string GetSafeNameForDeviceType(int devType)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>
            {
                { 1, "Push-button-Single" },
                { 2, "Push-button-Dual" },
                { 3, "Push-button-Quad" },
                { 5, "RoomController-w-Switch" },
                { 16, "SwitchingActuator" },
                { 17, "DimmingActuator" },
                { 18, "JalousieActuator" },
                { 19, "BinaryInput-230V" },
                { 20, "BinaryInput-Battery" },
                { 21, "RemoteControl-12Channel-old-design" },
                { 22, "Home-Manager" },
                { 23, "TemperatureInput" },
                { 24, "AnalogInput" },
                { 25, "AnalogActuator" },
                { 26, "Room-Manager" },
                { 27, "JalousieActuator-w-Security" },
                { 28, "CommunicationInterface" },
                { 29, "MotionDetector" },
                { 48, "RemoteControl-2Channel-small" },
                { 49, "RemoteControl-12Channel" },
                { 50, "RemoteControl-12Channel-w-display" },
                { 51, "RoomController-w-Switch-Humidity" },
                { 52, "Router" },
                { 53, "ImpulseInput" },
                { 54, "EMS" },
                { 55, "E-RaditorActuator" },
                { 56, "RemoteControl-Alarm-Pushbutton" },
                { 57, "BOSCOS" },
                { 62, "MEP" },
                { 65, "HRV" },
                { 68, "Rosetta-Sensor" },
                { 69, "Rosetta-Router" },
                { 71, "MultiChannelHeatingActuator" },
                { 72, "CommunicationInterfaceUSB" },
                { 74, "SwitchingActuator-NewGeneration" },
                { 75, "Router-NewGeneration" },
            };

            dict.TryGetValue(System.Convert.ToInt32(devType), out string tmp);
            return tmp;
        }

        private static string GetPropertyForMsgType(int msgType)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>
            {
                { 0x50, "MGW_RMT_ON" },
                { 0x51, "MGW_RMT_OFF" },
                { 0x52, "MGW_RMT_SWITCH_ON" },
                { 0x53, "MGW_RMT_SWITCH_OFF" },
                { 0x54, "MGW_RMT_UP_PRESSED" },
                { 0x55, "MGW_RMT_UP_RELEASED" },
                { 0x56, "MGW_RMT_DOWN_PRESSED" },
                { 0x57, "MGW_RMT_DOWN_RELEASED" },
                { 0x5A, "MGW_RMT_FORCED" },
                { 0x5B, "MGW_RMT_SINGLE_ON" },
                { 0x62, "MGW_RMT_VALUE" },
                { 0x63, "MGW_RMT_TOO_COLD" },
                { 0x64, "MGW_RMT_TOO_WARM" },
                { 0x70, "MGW_RMT_STATUS" },
                { 0x80, "MGW_RMT_BASIC_MODE" },
            };
            dict.TryGetValue(msgType, out string temp);
            return temp;
        }

        private static string GetPropertyForDataType(int dataType)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>
            {
                { 0x00, "MGW_RDT_NO_DATA" },
                { 0x01, "MGW_RDT_PERCENT" },
                { 0x02, "MGW_RDT_UINT8" },
                { 0x03, "MGW_RDT_INT16_1POINT" },
                { 0x04, "MGW_RDT_FLOAT" },
                { 0x0D, "MGW_RDT_UINT16" },
                { 0x21, "MGW_RDT_UINT16_1POINT" },
                { 0x22, "MGW_RDT_UINT16_2POINT" },
                { 0x23, "MGW_RDT_UINT16_3POINT" },
                { 0x0E, "MGW_RDT_UINT32" },
                { 0x0F, "MGW_RDT_UINT32_1POINT" },
                { 0x10, "MGW_RDT_UINT32_2POINT" },
                { 0x11, "MGW_RDT_UINT32_3POINT" },
                { 0x17, "MGW_RDT_RC_DATA" },
                { 0x1E, "MGW_RDT_TIME" },
                { 0x1F, "MGW_RDT_DATE" },
                { 0x35, "MGW_RDT_ROSETTA" }
            };
            dict.TryGetValue(dataType, out string temp);
            return temp;
        }

        public static string CreateAndListNodes()
        {
            /* There's going to be one node for every device type in use:
             * Dimmable actuators, switching actuators, push buttons, room controllers, etc
             * These will then be arrays where each datapoint is an item. */
            string nodes = "";


            List<PublishModel> pubList = new List<PublishModel>();
            List<DeviceType> activeTypes = new List<DeviceType>();
            foreach (Datapoint dp in CI.datapoints)
            {
                // Get the device type, add it to the list of active types (if it's not allready there)
                DeviceType devicetype = CI.devicetypes.Find(x => x.Number == dp.Type);
                if (!(devicetype != null && devicetype.Channels.Contains(dp.Channel) && devicetype.Modes.Contains(dp.Mode))) { continue; }
                if (!activeTypes.Contains(devicetype)) { activeTypes.Add(devicetype); }
            }

            foreach (DeviceType devType in activeTypes)
            {
                // Setting up the base topic string:     homie          /       supercar        / lights /
                string devName = Homie.GetSafeNameForDeviceType(devType.Number);

                string BaseTopic = ($"{Program.Settings.MQTT_BASETOPIC}/{Program.Settings.NAME}/").Replace("//", "/");

                // Add it to the list of node names.
                pubList.Add(new PublishModel($"{BaseTopic}/$nodes", $"{devName}[]"));                                       //homie/super-car/$nodes → "lights[]"

                // Update the base topic
                BaseTopic += ($"{devName}").Replace("//", "/");


                // Get all datapoints that belong to this specific devicetype
                List<Datapoint> dps = new List<Datapoint>();
                dps.AddRange(CI.datapoints.Where(x => x.Type == devType.Number && devType.Modes.Contains(x.Mode) && devType.Channels.Contains(x.Channel)));

                // Add the properties for this node
                string devProps = Homie.GetPropertiesForDeviceType(devType);
                HomieNodes.Add(new Homie.Node { PublishPath = BaseTopic, Name = devName, Array = $"0-{dps.Count - 1}", Properties = devProps });

                foreach (string p in devProps.Replace("[]", "").Split(","))
                {
                    // Each and every property must have their own set of MQTT messsages
                    HomieProperties.Add(Homie.GetHomiePropertyFor(devName, BaseTopic + "/" + p));

                    int devCnt = 0;
                    foreach (Datapoint datapoint in dps)
                    {
                        string dataAsString = "0";
                        Protocol.PT_RX.Packet pT_RX = datapoint.LatestDataValues;
                        if (datapoint.LatestDataValues != null)
                        {
                            dataAsString = CI.GetDataFromPacket(pT_RX.MGW_RX_DATA, pT_RX.MGW_RX_DATA_TYPE, p);
                        }
                        HomieArrayElements.Add(new Homie.ArrayElement { Name = datapoint.Name, PropertyName = p, Value = dataAsString, PublishPath = $"{BaseTopic}/{devName}_{devCnt}", BelongsToDP = datapoint.DP, ArrayIndex = devCnt });
                        devCnt++;
                    }
                }
            }
            return nodes;
        }


        public class Node
        {
            public string PublishPath { get; set; }
            public string Name { get; set; }            //homie/super-car/lights/$name → "Lights"
            public string Properties { get; set; }      //homie/super-car/lights/$properties → "intensity"
            public string Array { get; set; }           //homie/super-car/lights/$array → "0-1"
        }

        public class Property
        {
            public string PublishPath { get; set; }
            public string Name { get; set; }            //homie/super-car/lights/intensity/$name → "Intensity"
            public string Settable { get; set; }        //homie/super-car/lights/intensity/$settable → "true"
            public string Unit { get; set; }            //homie/super-car/lights/intensity/$unit → "%"
            public string DataType { get; set; }        //homie/super-car/lights/intensity/$datatype → "integer"
            public string Format { get; set; }          //homie/super-car/lights/intensity/$format → "0:100"
        }

        public class ArrayElement
        {
            public string PublishPath { get; set; }
            public string Name { get; set; }            //homie/super-car/lights_0/$name → "Back lights"
            public string PropertyName { get; set; }    
            public string Value { get; set; }           //homie/super-car/lights_0/intensity → "0"
            public int BelongsToDP { get; set; }
            public int ArrayIndex { get; set; }
        }

    }
}
