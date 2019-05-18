using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xComfortWingman
{
    public class Homie
    {
        public static List<Device> devices = new List<Device>();

        private static string LocalIP = "127.0.0.1";
        private static string MAC = "AA:BB:CC:DD:EE:FF";
        private static string Interval = Program.Settings.HOMIE_STATS_INTERVAL;

        public static List<Homie.NodeOld> HomieNodes = new List<Homie.NodeOld>();
        public static List<Homie.PropertyOld> HomieProperties = new List<Homie.PropertyOld>();
        public static List<Homie.ArrayElement> HomieArrayElements = new List<Homie.ArrayElement>();

        public static ArrayElement GetArrayElement(int DP)
        {
            return HomieArrayElements.Find(x => x.BelongsToDP == DP);
        }

        public static void UpdateArrayElement(int DP, string dataValue)
        {
            UpdateArrayElement(GetArrayElement(DP), dataValue);
        }
        public static void UpdateArrayElement(ArrayElement arrayElement, string dataValue)
        {
            arrayElement.Value = dataValue;
        }

        public static List<PublishModel> PropertyDetails(string propName, string dpName)
        {
            List<PublishModel> props = new List<PublishModel>();
            string switcher = propName;
            //               /dpName/propName /
            props.Add(new PublishModel($"{dpName}/{propName}/$name", propName));                            //homie/super-car/lights/intensity/$name → "Intensity"
                                                                                                            //homie/super-car/lights/intensity/$settable → "true"
                                                                                                            //homie/super-car/lights/intensity/$unit → "%"
                                                                                                            //homie/super-car/lights/intensity/$datatype → "integer"
                                                                                                            //homie/super-car/lights/intensity/$format → "0:100"
            switch (switcher)
            {
                case "power":
                    {
                        props.Add(new PublishModel($"{dpName}/{propName}/$settable", "true"));
                        props.Add(new PublishModel($"{dpName}/{propName}/$datatype", "boolean"));
                        break;
                    }
                case "intensity":
                    {
                        props.Add(new PublishModel($"{dpName}/{propName}/$settable", "true"));
                        props.Add(new PublishModel($"{dpName}/{propName}/$unit", "%"));
                        props.Add(new PublishModel($"{dpName}/{propName}/$datatype", "integer"));
                        props.Add(new PublishModel($"{dpName}/{propName}/$format", "0-100"));
                        break;
                    }
                case "jalousie":
                    {
                        props.Add(new PublishModel($"{dpName}/{propName}/$settable", "true"));
                        props.Add(new PublishModel($"{dpName}/{propName}/$datatype", "string"));
                        break;
                    }
                case "buttonstate":
                    {
                        props.Add(new PublishModel($"{dpName}/{propName}/$settable", "false"));
                        props.Add(new PublishModel($"{dpName}/{propName}/$datatype", "string"));
                        break;
                    }
                case "wheelposition":
                    {
                        props.Add(new PublishModel($"{dpName}/{propName}/$settable", "false"));
                        props.Add(new PublishModel($"{dpName}/{propName}/$datatype", "float"));
                        props.Add(new PublishModel($"{dpName}/{propName}/$format", "-15:3"));
                        break;
                    }
                case "temperature":
                    {
                        props.Add(new PublishModel($"{dpName}/{propName}/$settable", "false"));
                        props.Add(new PublishModel($"{dpName}/{propName}/$unit", "°C"));
                        props.Add(new PublishModel($"{dpName}/{propName}/$datatype", "float"));
                        props.Add(new PublishModel($"{dpName}/{propName}/$format", "-50:180"));
                        break;
                    }
                case "humidity":
                    {
                        props.Add(new PublishModel($"{dpName}/{propName}/$settable", "false"));
                        props.Add(new PublishModel($"{dpName}/{propName}/$unit", "%"));
                        props.Add(new PublishModel($"{dpName}/{propName}/$datatype", "integer"));
                        props.Add(new PublishModel($"{dpName}/{propName}/$format", "0-100"));
                        break;
                    }
                case "battery":
                    {
                        props.Add(new PublishModel($"{dpName}/{propName}/$settable", "false"));
                        props.Add(new PublishModel($"{dpName}/{propName}/$datatype", "integer"));
                        props.Add(new PublishModel($"{dpName}/{propName}/$format", "0-10"));
                        break;
                    }
                case "signal":
                    {
                        props.Add(new PublishModel($"{dpName}/{propName}/$settable", "false"));
                        props.Add(new PublishModel($"{dpName}/{propName}/$unit", "-dBm"));
                        props.Add(new PublishModel($"{dpName}/{propName}/$datatype", "integer"));
                        props.Add(new PublishModel($"{dpName}/{propName}/$format", "0-120"));
                        break;
                    }
                default:
                    {
                        // This is the bare minimum required to move the data through the system so that it'll be
                        // possible to handle those special devices in the next system...
                        props.Add(new PublishModel($"{dpName}/{propName}/$settable", "false"));
                        props.Add(new PublishModel($"{dpName}/{propName}/$datatype", "string"));
                        break;
                    }
            }
            return props;
        }

        public static PropertyOld GetHomiePropertyFor(string propertyName, string baseTopic)
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
                        return new PropertyOld { Name = propertyName, Settable = "true", DataType = "boolean", PublishPath = baseTopic };
                    }
                case "intensity":
                    {
                        return new PropertyOld { Name = propertyName, Settable = "true", Unit = "%", DataType = "boolean", Format = "0-100", PublishPath = baseTopic };
                    }
                case "jalousie":
                    {
                        return new PropertyOld { Name = propertyName, Settable = "true", DataType = "string", PublishPath = baseTopic };
                    }
                case "buttonstate":
                    {
                        return new PropertyOld { Name = propertyName, Settable = "false", DataType = "string", PublishPath = baseTopic };
                    }
                case "wheelposition":
                    {
                        return new PropertyOld { Name = propertyName, Settable = "false", DataType = "float", Format = "-15:3", PublishPath = baseTopic };
                    }
                case "temperature":
                    {
                        return new PropertyOld { Name = propertyName, Settable = "true", DataType = "boolean", PublishPath = baseTopic };
                    }
                case "humidity":
                    {
                        return new PropertyOld { Name = propertyName, Settable = "false", Unit = "%", DataType = "integer", Format = "0-100", PublishPath = baseTopic };
                    }
                case "battery":
                    {
                        return new PropertyOld { Name = propertyName, Settable = "false", DataType = "integer", Format = "0-10", PublishPath = baseTopic };
                    }
                case "signal":
                    {
                        return new PropertyOld { Name = propertyName, Settable = "false", Unit = "-dBm", DataType = "integer", Format = "0-120", PublishPath = baseTopic };
                    }
                default:
                    {
                        // This is the bare minimum required to move the data through the system so that it'll be
                        // possible to handle those special devices in the next system...
                        return new PropertyOld { Name = propertyName, Settable = "false", DataType = "string", PublishPath = baseTopic };
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
                { 1, "PushButtonSingle" },
                { 2, "PushButtonDual" },
                { 3, "PushButtonQuad" },
                { 5, "RoomControllerWSwitch" },
                { 16, "SwitchingActuator" },
                { 17, "DimmingActuator" },
                { 18, "JalousieActuator" },
                { 19, "BinaryInput230V" },
                { 20, "BinaryInputBattery" },
                { 21, "RemoteControl12ChannelOldDesign" },
                { 22, "HomeManager" },
                { 23, "TemperatureInput" },
                { 24, "AnalogInput" },
                { 25, "AnalogActuator" },
                { 26, "RoomManager" },
                { 27, "JalousieActuatorWSecurity" },
                { 28, "CommunicationInterface" },
                { 29, "MotionDetector" },
                { 48, "RemoteControl2ChannelSmall" },
                { 49, "RemoteControl12Channel" },
                { 50, "RemoteControl12ChannelWDisplay" },
                { 51, "RoomControllerWSwitchHumidity" },
                { 52, "Router" },
                { 53, "ImpulseInput" },
                { 54, "EMS" },
                { 55, "ERaditorActuator" },
                { 56, "RemoteControlAlarmPushbutton" },
                { 57, "BOSCOS" },
                { 62, "MEP" },
                { 65, "HRV" },
                { 68, "RosettaSensor" },
                { 69, "RosettaRouter" },
                { 71, "MultiChannelHeatingActuator" },
                { 72, "CommunicationInterfaceUSB" },
                { 74, "SwitchingActuatorNewGeneration" },
                { 75, "RouterNewGeneration" },
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

        //public static string CreateAndListNodes()
        //{
        //    /* There's going to be one node for every device type in use:
        //     * Dimmable actuators, switching actuators, push buttons, room controllers, etc
        //     * These will then be arrays where each datapoint is an item. */
        //    string nodes = "";


        //    List<PublishModel> pubList = new List<PublishModel>();
        //    List<DeviceType> activeTypes = new List<DeviceType>();
        //    foreach (Datapoint dp in CI.datapoints)
        //    {
        //        // Get the device type, add it to the list of active types (if it's not allready there)
        //        System.Diagnostics.Debug.WriteLine(dp.Name + " is DP type: " + dp.Type);
        //        DeviceType devicetype = CI.devicetypes.Find(x => x.Number == dp.Type && x.Channels.Contains(dp.Channel) && x.Modes.Contains(dp.Mode));
        //        //if (!(devicetype != null && devicetype.Channels.Contains(dp.Channel) && devicetype.Modes.Contains(dp.Mode))) { continue; }
        //        if (devicetype != null && !activeTypes.Contains(devicetype)) { activeTypes.Add(devicetype); }
        //    }

        //    foreach (DeviceType devType in activeTypes)
        //    {
        //        // Setting up the base topic string:     homie          /       supercar        / lights /
        //        string devName = Homie.GetSafeNameForDeviceType(devType.Number);

        //        string BaseTopic = ($"{Program.Settings.MQTT_BASETOPIC}/{Program.Settings.NAME}/").Replace("//", "/");

        //        // Add it to the list of node names.
        //        //pubList.Add(new PublishModel($"{BaseTopic}/$nodes", $"{devName}YY[]"));                                       //homie/super-car/$nodes → "lights[]"

        //        // Update the base topic
        //        BaseTopic = $"{devName}";//.Replace("//", "/");


        //        // Get all datapoints that belong to this specific devicetype
        //        List<Datapoint> dps = new List<Datapoint>();
        //        dps.AddRange(CI.datapoints.Where(x => x.Type == devType.Number && devType.Modes.Contains(x.Mode) && devType.Channels.Contains(x.Channel)));

        //        // Add the properties for this node
        //        string devProps = Homie.GetPropertiesForDeviceType(devType);
        //        HomieNodes.Add(new Homie.Node { PublishPath = BaseTopic, Name = devName, Array = $"0-{dps.Count - 1}", Properties = devProps });

        //        foreach (string p in devProps.Replace("[]", "").Split(","))
        //        {
        //            // Each and every property must have their own set of MQTT messsages
        //            HomieProperties.Add(Homie.GetHomiePropertyFor(devName, BaseTopic + "/" + p));

        //            int devCnt = 0;
        //            foreach (Datapoint datapoint in dps)
        //            {
        //                string dataAsString = "0";
        //                Protocol.PT_RX.Packet pT_RX = datapoint.LatestDataValues;
        //                if (datapoint.LatestDataValues != null)
        //                {
        //                    dataAsString = CI.GetDataFromPacket(pT_RX.MGW_RX_DATA, pT_RX.MGW_RX_DATA_TYPE, p);
        //                }
        //                HomieArrayElements.Add(new Homie.ArrayElement { Name = datapoint.Name, PropertyName = p, Value = dataAsString, PublishPath = $"{BaseTopic}/{devName}_{devCnt}", BelongsToDP = datapoint.DP, ArrayIndex = devCnt });
        //                devCnt++;
        //            }
        //        }
        //    }
        //    return nodes;
        //}


        //      Non-array solution:

        //homie/super-car/$nodes → "wheels,ENGINE,lights[]"
        //                                     /
        //                    -----------------
        //                   /
        //homie/super-car/ENGINE/$name → "Car engine"
        //homie/super-car/ENGINE/$type → "V8"
        //homie/super-car/ENGINE/$properties → "speed,direction,temperature"
        //                   |                                      /
        //                   |         -----------------------------
        //                   |        /
        //homie/super-car/ENGINE/temperature/$name → "Engine temperature"
        //homie/super-car/ENGINE/temperature/$settable → "false"
        //homie/super-car/ENGINE/temperature/$unit → "°C"
        //homie/super-car/ENGINE/temperature/$datatype → "float"
        //homie/super-car/ENGINE/temperature/$format → "-20:120"
        //
        //homie/super-car/ENGINE/temperature → "21.5"


        public static async System.Threading.Tasks.Task PublishDatapointAsNode(Datapoint datapoint)
        {
            // First, we need a place to store all of the MQTT messages we need to send, then send them all at once.
            List<PublishModel> publishModels = new List<PublishModel>();
            string NodeName = (SanitiseString(datapoint.Name)) + "/";

            // Get the correct device type for the datapoint
            DeviceType devicetype = CI.devicetypes.Find(x => x.Number == datapoint.Type && x.Channels.Contains(datapoint.Channel) && x.Modes.Contains(datapoint.Mode));

            // Add the properties for this node/device
            string typeProperties = Homie.GetPropertiesForDeviceType(devicetype);

            publishModels.Add(new PublishModel(NodeName + "$name", datapoint.Name));
            publishModels.Add(new PublishModel(NodeName + "$type", devicetype.Name));
            publishModels.Add(new PublishModel(NodeName + "$properties", typeProperties));

            foreach (string property in typeProperties.Split(","))
            {
                // This adds the 5 property messages
                publishModels.AddRange(PropertyDetails(property, Homie.SanitiseString(datapoint.Name)));

                // Now, all we ned is the data...
                string dataAsString = CI.GetDataFromPacket(datapoint.LatestDataValues.MGW_RX_DATA, datapoint.LatestDataValues.MGW_RX_DATA_TYPE, "");
                if (datapoint.LatestDataValues.MGW_RX_DATA_TYPE == 0x00) { dataAsString = Protocol.PT_RX.MGW_RX_MSG_TYPE.GetNameFromByte(datapoint.LatestDataValues.MGW_RX_MSG_TYPE); }

                if (property == "signal") { dataAsString = datapoint.LatestDataValues.MGW_RX_RSSI.ToString(); }         // Override for the two properties where the data isn't in the normal place
                if (property == "battery") { dataAsString = datapoint.LatestDataValues.MGW_RX_BATTERY.ToString(); }
                if (dataAsString != "") { publishModels.Add(new PublishModel(NodeName + property, dataAsString)); }
            }


            foreach (PublishModel pub in publishModels)
            {
                //Console.WriteLine(pub.PublishPath + "\t\t\t" + pub.Payload);
                await MQTT.SendMQTTMessageAsync(pub.PublishPath, pub.Payload, true);
            }
        }

        public static async Task PublishStats()
        {
            string[] stats = new string[] { GetStatsUptime(), GetStatsSignal(), GetStatsCPUtemp(), GetStatsCPUload(), GetStatsBattery(), GetStatsFreeHeap(), GetStatsSupply() };
            foreach (Device device in devices)
            {
                //int index = 0;
                await MQTT.SendMQTTMessageAsync($"{device.Name}/$stats/uptime", stats[0], true);
                //await MQTT.SendMQTTMessageAsync($"{device.Name}/$stats/signal", stats[1], true);
                //await MQTT.SendMQTTMessageAsync($"{device.Name}/$stats/cputemp", stats[2], true);
                //await MQTT.SendMQTTMessageAsync($"{device.Name}/$stats/cpuload", stats[3], true);
                //await MQTT.SendMQTTMessageAsync($"{device.Name}/$stats/battery", stats[4], true);
                //await MQTT.SendMQTTMessageAsync($"{device.Name}/$stats/freeheap", stats[5], true);
                //await MQTT.SendMQTTMessageAsync($"{device.Name}/$stats/supply", stats[6], true);
            }
            // These must be sent with the interval given in homie/unitname/$interval
            
        }

        #region "Stats helpers"

        private static string GetStatsUptime()
        {
            return DateTime.Now.Subtract(Program.ApplicationStart).TotalSeconds.ToString();
        }
        private static string GetStatsSignal()
        {
            return "0%";
        }
        private static string GetStatsCPUtemp()
        {
            try
            {
                string temp = "0";
                using (StreamReader r = new StreamReader("/sys/class/thermal/thermal_zone0/temp"))
                {
                    temp = r.ReadToEnd();
                }
                return temp;
            }
            catch
            {
                return "-1";
            }
        }
        private static string GetStatsCPUload()
        {
            return "0";
        }
        private static string GetStatsBattery()
        {
            return "0%";
        }
        private static string GetStatsFreeHeap()
        {
            return "0";
        }
        private static string GetStatsSupply()
        {
            return "0";
        }
        #endregion

        public static string SanitiseString(string str)
        {
            return System.Text.RegularExpressions.Regex.Replace(str, "[^a-zA-Z0-9]+", "", System.Text.RegularExpressions.RegexOptions.Compiled);
        }

        //public static void AddDatapointAsDevice(Datapoint datapoint)
        //{
        //    string nodes = GetNodesStringForDatapoint(datapoint);
        //    List<Node> NodeList = new List<Node>();
        //    foreach (string nod in GetNodesStringForDatapoint(datapoint).Split(","))
        //    {
        //        NodeList.Add(GetNodeFromNodeName(nod));
        //    }

        //    devices.Add(new Device
        //    {
        //        Datapoint = datapoint,
        //        Name = SanitiseString(datapoint.Name),
        //        State="init",
        //        Nodes=nodes,
        //        Node=NodeList.ToArray()
        //        // The rest of the values are defaults
        //    });
        //}



        public class NodeOld
        {
            public string Name { get; set; }            //homie/super-car/lights/$name → "Lights"
            public string Properties { get; set; }      //homie/super-car/lights/$properties → "intensity"
            public string Array { get; set; }           //homie/super-car/lights/$array → "0-1"                     // Apparently, these don't work. That's about 25hrs lost...

            // Just to prevent errors
            public string PublishPath { get; set; }

            // Not part of the Homie specs
            public PropertyOld[] Property { get; set; }
        }


        public class PropertyOld
        {
            public string DataValue { get; set; }       //homie/super-car/lights/intensity → "42"


            public string DataType { get; set; }
            public string Format { get; set; }
            public string Name { get; set; }
            public string PublishPath { get; set; }
            public string Settable { get; set; }
            public string Unit { get; set; }

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


        //      DEVICE
        //homie/ThermoBathroomF0
        //homie/ThermoBathroomF0/$homie										=	"3.0.1"
        //homie/ThermoBathroomF0/$name 										=	"Thermostat bathroom"
        //homie/ThermoBathroomF0/$localip									=	"0.0.0.0"
        //homie/ThermoBathroomF0/$mac										=	"00:00:00:00:00:00"
        //homie/ThermoBathroomF0/$fw/name									=	"BachelorPad"
        //homie/ThermoBathroomF0/$fw/version								=	"1.0.0"
        //homie/ThermoBathroomF0/$nodes										=	"temperature,wheelposition,signal,mode"
        //homie/ThermoBathroomF0/$implementation							=	"xComfort"
        //homie/ThermoBathroomF0/$stats/interval							=	"60"
        //homie/ThermoBathroomF0/$state										=	"ready"


        //						 NODE
        //homie/ThermoBathroomF0/temperature
        //homie/ThermoBathroomF0/wheelposition
        //homie/ThermoBathroomF0/mode
        //homie/ThermoBathroomF0/signal

        //homie/ThermoBathroomF0/temperature/$name              			=   "Temperature reading"
        //homie/ThermoBathroomF0/temperature/$type              			=   "Thermostat"
        //homie/ThermoBathroomF0/temperature/$properties        			=   "temperature"

        //homie/ThermoBathroomF0/wheelposition/$name            			=   "Wheel position"
        //homie/ThermoBathroomF0/wheelposition/$type            			=   "wheelposition"
        //homie/ThermoBathroomF0/wheelposition/$properties      			=   "position"

        //homie/ThermoBathroomF0/mode/$name									=   "Signal mode"
        //homie/ThermoBathroomF0/mode/$type									=   "Mode"
        //homie/ThermoBathroomF0/mode/$properties							=   "integer"

        //homie/ThermoBathroomF0/signal/$name								=   "Signal strength"
        //homie/ThermoBathroomF0/signal/$type								=   "RSSI"
        //homie/ThermoBathroomF0/signal/$properties							=   "strength"


        //									 PROPERTY
        //homie/ThermoBathroomF0/temperature/temperature/$name				=   "Temperature"
        //homie/ThermoBathroomF0/temperature/temperature/$settable			=   "false"
        //homie/ThermoBathroomF0/temperature/temperature/$unit				=   "C"
        //homie/ThermoBathroomF0/temperature/temperature/$datatype			=   "integer"
        //homie/ThermoBathroomF0/temperature/temperature/$format			=   "0:100"

        //homie/ThermoBathroomF0/wheelposition/position/$name				=   "Wheel pos"
        //homie/ThermoBathroomF0/wheelposition/position/$settable			=   "false"
        //homie/ThermoBathroomF0/wheelposition/position/$unit				=   ""
        //homie/ThermoBathroomF0/wheelposition/position/$datatype			=   "float"
        //homie/ThermoBathroomF0/wheelposition/position/$format				=   "-9:3"

        public class Device // Represents a Datapoint. Datapoints represents xComfort devices.
        {
            public string Homie { get; set; }  =  Program.Settings.HOMIE_HOMIE;                        //homie/DimKitchen/$homie			"3.0.1"
            public string Name  { get; set; }                                                               //homie/DimKitchen/$name 			"Kitchen lights"
            public string Localip { get; set; } = LocalIP;                                                  //homie/DimKitchen/$localip			"0.0.0.0"
            public string Mac  { get; set; } = MAC;                                                         //homie/DimKitchen/$mac				"00:00:00:00:00:00"
            public string Fw_name  { get; set; } =   Program.Settings.HOMIE_FW_NAME;                   //homie/DimKitchen/$fw/name			"BachelorPad"
            public string Fw_version  { get; set; } =   Program.Settings.HOMIE_FW_VERSION;             //homie/DimKitchen/$fw/version		"1.0.0"
            public string Nodes  { get; set; }                                                              //homie/DimKitchen/$nodes			"lights,signal"
            public string Implementation  { get; set; } =  Program.Settings.HOMIE_IMPLEMENTATION;      //homie/DimKitchen/$implementation	"xComfort"
            public string Stats_interval  { get; set; } =   Program.Settings.HOMIE_STATS_INTERVAL;     //homie/DimKitchen/$stats/interval	"60"
            public string State  { get; set; }                                                              //homie/DimKitchen/$state			"ready"

            public List<Node> Node { get; set; } = new List<Node>();

            // Not part of the Homie specs
            public Datapoint Datapoint { get; set; }
        }

        public class Node
        {
            public string Name { get; set; }                                                                //homie/DimKitchen/lights/$name         =   "Lights"
            public string Type { get; set; }                                                                //homie/DimKitchen/lights/$type         =   "Dimmer"
            public string Properties { get; set; }                                                          //homie/DimKitchen/lights/$properties   =   "intensity"
            // Apparently, Arrays don't work. That's about 25hrs lost...
            //public string Array { get; set; }                                                             //homie/DimKitchen/lights/$array        =   "0-1"

            // Not part of the Homie specs
            public string Value { get; set; }
            public string PathName { get; set; }
            public List<Property> PropertyList { get; set; }
        }

        public class Nodes
        {
            public static readonly Node nodeSwitching = new Node { PathName= "power", Name = "Switching actuator", Type = "Toggle", Properties = "power", PropertyList = new List<Property> {  Properties.propPower } };
            public static readonly Node nodeDimming = new Node { PathName = "dimlevel", Name = "Dimming actuator", Type = "Dimmer", Properties = "dimlevel", PropertyList = new List<Property> {  Properties.propDimlevel } };
            public static readonly Node nodeJalousie = new Node { PathName = "jalousie", Name = "Jalousie actuator", Type = "Jalousie", Properties = "jalousie", PropertyList = new List<Property>{ Properties.propJalousie } };
            public static readonly Node nodeTemperature = new Node { PathName = "temperature", Name = "Temperature", Type = "Temp", Properties = "temperature", PropertyList = new List<Property> {  Properties.propTemperature } };
            public static readonly Node nodeHumidity = new Node { PathName = "humidity", Name = "Humidity", Type = "Humidity", Properties = "humidity", PropertyList = new List<Property> { Properties.propHumidity } };
            public static readonly Node nodeWheelPosition = new Node { PathName = "wheelposition", Name = "WheelPosition", Type = "Wheel", Properties = "wheelposition", PropertyList = new List<Property> {  Properties.propWheelposition } };
            public static readonly Node nodeButtonState = new Node { PathName = "buttonstate", Name = "ButtonState", Type = "Button", Properties = "buttonstate", PropertyList = new List<Property> {  Properties.propButtonstate } };
            public static readonly Node nodeSignal = new Node { PathName = "signal", Name = "Signal", Type = "RSSI", Properties = "signal", PropertyList = new List<Property> { Properties.propSignal } };
            public static readonly Node nodeBattery = new Node { PathName = "battery", Name = "Battery", Type = "Battery", Properties = "battery", PropertyList = new List<Property> { Properties.propBattery } };
            public static readonly Node nodeGeneric = new Node { PathName = "generic", Name = "Generic", Type = "Generic", Properties = "signal", PropertyList = new List<Property> { Properties.propSignal } };

            public static readonly Node nodeRoomController1 = new Node { PathName = "roomcontroller1", Name = "RoomController1", Type = "RC1", Properties = "temperature,wheelposition", PropertyList = new List<Property> { Properties.propTemperature, Properties.propWheelposition } };
            public static readonly Node nodeRoomController2 = new Node { PathName = "roomcontroller2", Name = "RoomController2", Type = "RC2", Properties = "temperature,wheelposition,humidity", PropertyList = new List<Property> { Properties.propTemperature, Properties.propWheelposition, Properties.propHumidity } };

        }

        public class Property
        {
            public string DataValue { get; set; }                                                           //homie/DimKitchen/lights/intensity     =   "42"

            public string Name { get; set; }                                                                //homie/DimKitchen/lights/intensity/$name       =   "Light Intensity"
            public string Settable { get; set; }                                                            //homie/DimKitchen/lights/intensity/$settable=  =   "true"
            public string Unit { get; set; }                                                                //homie/DimKitchen/lights/intensity/$unit       =   "%"
            public string DataType { get; set; }                                                            //homie/DimKitchen/lights/intensity/$datatype   =   "integer"
            public string Format { get; set; }                                                              //homie/DimKitchen/lights/intensity/$format     =   "0:100"

            public string PublishPath { get; set; }
            public string PathName { get; set; }
        }

        public class Properties
        {
            public static readonly Property propPower = new Property { PathName="power", Name = "Power state", Settable = "true", DataType = "boolean" };
            public static readonly Property propDimlevel = new Property { PathName = "dimlevel", Name = "Level", Settable = "true", Unit = "%", DataType = "integer", Format = "0-100"};
            public static readonly Property propJalousie = new Property { PathName = "jalousie", Name = "Shutter state", Settable = "true", DataType = "string" };
            public static readonly Property propButtonstate = new Property { PathName = "buttonstate", Name = "Button state", Settable = "false", DataType = "string" };
            public static readonly Property propWheelposition = new Property { PathName = "wheelposition", Name = "Wheel setting", Settable = "false", DataType = "float", Format = "-15:3" };
            public static readonly Property propTemperature = new Property { PathName = "temperature", Name = "Temperature reading", Settable = "false", DataType = "float", Format= "-40:180" };
            public static readonly Property propHumidity = new Property { PathName = "humidity", Name = "Humidity reading", Settable = "false", Unit = "%", DataType = "integer", Format = "0-100" };
            public static readonly Property propBattery = new Property { PathName = "battery", Name = "Battery level", Settable = "false", DataType = "integer", Format = "0-10" };
            public static readonly Property propSignal = new Property { PathName = "signal", Name = "Signal strength", Settable = "false", Unit = "-dBm", DataType = "integer", Format = "0-120" };
        }

        #region "Device helpers"

        public static Device GetDeviceFromDatapoint(Datapoint datapoint)
        {
            Device device = new Device();
            device.Datapoint = datapoint;
            device.Name = SanitiseString(datapoint.Name);
            device.Nodes = GetNodesStringForDatapoint(datapoint);
            device.Node = GetNodesListForDatapoint(datapoint);
            //foreach (string nodeName in device.Nodes.Split(','))
            //{
            //    device.Node.Add(GetNodeFromNodeName(nodeName));
            //}
            device.State = "ready";
            return device;
        }

        public static string GetNodesStringForDatapoint(Datapoint datapoint)
        {
            // The DeviceType determines what nodes are available for any given device.
            DeviceType deviceType = CI.devicetypes.Find(x => x.Number == datapoint.Type && x.Channels.Contains(datapoint.Channel) && x.Modes.Contains(datapoint.Mode));
            
            // Some devices are really easy, get those out of the way first...
            switch (deviceType.Number)
            {
                // Output-devices
                case 46:
                case 16: { return "power,signal"; }
                case 17: { return "dimlevel,signal"; }
                case 27:
                case 18: { return "jalousie,signal"; }

                // Input-devices
                case 1:         // Push buttons
                case 2:
                case 3:         
                case 19:        // Binary inputs
                case 20:        
                case 21:
                case 48:        // Remote controls
                case 49:        
                case 50:
                case 56: { return "buttonstate,battery,signal"; }
                case 5:  { return "temperature,wheelposition,battery,signal"; }
                case 51: { return "temperature,wheelposition,humidity,battery,signal"; }
                case 23: { return "temperature,battery,signal"; }
                case 22: { return "power,signal"; }     // Home manager
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
            if (tmp.EndsWith(',')) { tmp = tmp.Remove(tmp.Length - 1); }
            return tmp;
        }

        public static async Task UpdateDeviceData(Device device, Protocol.PT_RX.Packet packet)
        {
            device.Datapoint.LatestDataValues = packet;
            device.Datapoint.LastUpdate = DateTime.Now;
                        //string newData = CI.GetDataFromPacket(packet.MGW_RX_DATA, packet.MGW_RX_DATA_TYPE, "");
            foreach (Node node in device.Node)
            {
                switch (node.Type)
                {
                    case "RSSI": { node.Value = packet.MGW_RX_RSSI.ToString(); break; }
                    case "Battery": { node.Value = packet.MGW_RX_BATTERY.ToString(); break; }
                    default: { node.Value = CI.GetDataFromPacket(packet.MGW_RX_DATA, packet.MGW_RX_DATA_TYPE, ""); break; }
                }
                MyLogger.DoLog($"Updating data for {device.Name}'s {node.Name}: {node.Value}...",4);
                await MQTT.SendMQTTMessageAsync($"{device.Name}/{node.PathName}/{node.PropertyList[0].PathName}", node.Value, true);
            }
        }

        public static Node GetNodeFromNodeName(string nodeName)
        {
            Node nodePower = new Node { Name = "power", Type = "SwitchingActuator", PropertyList = new List<Property> { Properties.propPower }, Properties = "power" };
            Node nodeDimLevel = new Node { Name = "dimlevel", Type = "DimmingActuator", PropertyList = new List<Property> { Properties.propDimlevel }, Properties = "dimlevel" };
            Node nodeJalousie = new Node { Name = "jalousie", Type = "JalousieActuator", PropertyList = new List<Property> { Properties.propJalousie }, Properties = "jalousie" };
            Node nodeButtonstate = new Node { Name = "buttonstate", Type = "PushButton", PropertyList = new List<Property> { Properties.propButtonstate }, Properties = "buttonstate" };
            Node nodeTemperature = new Node { Name = "temperature", Type = "TemperatureSensor", PropertyList = new List<Property> { Properties.propTemperature }, Properties = "temperature" };
            Node nodeSignal = new Node { Name = "signal", Type = "Any", PropertyList = new List<Property> { Properties.propSignal }, Properties = "signal" };
            Node nodeBattery = new Node { Name = "battery", Type = "WirelessDevice", PropertyList = new List<Property> { Properties.propBattery }, Properties = "battery" };
            Node nodeWheelposition = new Node { Name = "wheelposition", Type = "RoomController1", PropertyList = new List<Property> { Properties.propWheelposition }, Properties = "wheelposition" };
            Node nodeHumidity = new Node { Name = "humidity", Type = "RoomController2", PropertyList = new List<Property> { Properties.propHumidity }, Properties = "humidity" };

            Node nodeRoomController1 = new Node { Name = "RoomController", Type = "RoomController", PropertyList = new List<Property> { Properties.propWheelposition, Properties.propTemperature }, Properties = "wheelposition, temperature" };
            Node nodeRoomController2 = new Node { Name = "RoomControllerWHumidity", Type = "RoomControllerWHumidity", PropertyList = new List<Property> { Properties.propHumidity }, Properties = "humidity" };

            switch (nodeName)
            {
                case "power": { return nodePower; }
                case "dimlevel": { return nodeDimLevel; }
                case "jalousie": { return nodeJalousie; }
                case "buttonstate": { return nodeButtonstate; }
                case "temperature": { return nodeTemperature; }
                case "wheelposition": { return nodeWheelposition; }
                case "humidity": { return nodeHumidity; }
                case "signal": { return nodeSignal; }
                case "battery": { return nodeBattery; }
                case "RoomController1": { return nodeRoomController1; }
                case "RoomController2": { return nodeRoomController2; }
                default:
                    {
                        return null;
                    }
            }

        }

        public static List<Node> GetNodesListForDatapoint(Datapoint datapoint)
        {
            // The DeviceType determines what nodes are available for any given device.
            DeviceType deviceType = CI.devicetypes.Find(x => x.Number == datapoint.Type && x.Channels.Contains(datapoint.Channel) && x.Modes.Contains(datapoint.Mode));
            switch (deviceType.Number)
            {
                // Output-devices
                case 46:
                case 16: { return new List<Node> { Nodes.nodeSignal, Nodes.nodeSwitching }; }
                case 17: { return new List<Node> { Nodes.nodeSignal, Nodes.nodeDimming }; }
                case 27:
                case 18: { return new List<Node> { Nodes.nodeSignal, Nodes.nodeJalousie }; }

                // Input-devices
                case 1:         // Push buttons
                case 2:
                case 3:
                case 19:        // Binary inputs
                case 20:
                case 21:
                case 48:        // Remote controls
                case 49:
                case 50:
                case 56: { return new List<Node> { Nodes.nodeSignal, Nodes.nodeBattery, Nodes.nodeButtonState }; }
                case 5:  { return new List<Node> { Nodes.nodeSignal, Nodes.nodeBattery, Nodes.nodeRoomController1 }; }
                case 51: { return new List<Node> { Nodes.nodeSignal, Nodes.nodeBattery, Nodes.nodeRoomController2 }; }
                case 23: { return new List<Node> { Nodes.nodeSignal, Nodes.nodeBattery, Nodes.nodeTemperature }; }
                case 22: { return new List<Node> { Nodes.nodeSignal }; }
                default:
                    {
                        return new List<Node> { Nodes.nodeSignal, Nodes.nodeGeneric };
                    }
            }
          
            //// For the rest, return the name of their data and message type...
            //string tmp = "battery,signal,";
            //foreach (byte dataType in deviceType.DataTypes)
            //{
            //    if (dataType == Protocol.PT_RX.MGW_RX_DATA_TYPE.MGW_RDT_NO_DATA)
            //    {
            //        foreach (byte msgType in deviceType.MessageTypes)
            //        {
            //            tmp += (Protocol.PT_RX.MGW_RX_MSG_TYPE.GetTechnicalNameFromByte(msgType)) + ",";
            //        }
            //    }
            //    else
            //    {
            //        tmp += (Protocol.PT_RX.MGW_RX_DATA_TYPE.GetNameFromByte(dataType)) + ",";
            //    }
            //}
            //if (tmp.EndsWith(',')) { tmp = tmp.Remove(tmp.Length - 1); }
            //return tmp;
        }

        //public static string GetPropertiesForDatapoint(Datapoint datapoint)
        //{

        //    switch("test")
        //    {
        //        case "power":
        //            {
        //            return new Property { Name = nodeName, Settable = "true", DataType = "boolean", PublishPath = publishPath };
        //        }
        //        case "intensity":
        //            {
        //            return new Property { Name = nodeName, Settable = "true", Unit = "%", DataType = "boolean", Format = "0-100", PublishPath = publishPath };
        //        }
        //        case "jalousie":
        //            {
        //            return new Property { Name = nodeName, Settable = "true", DataType = "string", PublishPath = publishPath };
        //        }
        //        case "buttonstate":
        //            {
        //            return new Property { Name = nodeName, Settable = "false", DataType = "string", PublishPath = publishPath };
        //        }
        //        case "wheelposition":
        //            {
        //            return new Property { Name = nodeName, Settable = "false", DataType = "float", Format = "-15:3", PublishPath = publishPath };
        //        }
        //        case "temperature":
        //            {
        //            return new Property { Name = nodeName, Settable = "true", DataType = "boolean", PublishPath = publishPath };
        //        }
        //        case "humidity":
        //            {
        //            return new Property { Name = nodeName, Settable = "false", Unit = "%", DataType = "integer", Format = "0-100", PublishPath = publishPath };
        //        }
        //        case "battery":
        //            {
        //            return new Property { Name = nodeName, Settable = "false", DataType = "integer", Format = "0-10", PublishPath = publishPath };
        //        }
        //        case "signal":
        //            {
        //            return new Property { Name = nodeName, Settable = "false", Unit = "-dBm", DataType = "integer", Format = "0-120", PublishPath = publishPath };
        //        }
        //        default:
        //            {
        //            // This is the bare minimum required to move the data through the system so that it'll be
        //            // possible to handle those special devices in the next system...
        //            return new Property { Name = nodeName, Settable = "false", DataType = "string", PublishPath = publishPath };
        //        }
        //    }
        //}

        //public static Node GetNodeFromNodeNameX(string nodeName)
        //{
        //    List<PublishModel> props = new List<PublishModel>();

        //    //homie/DEVICE    /NODE  /PROPERTY

        //    //      DEVICE
        //    //homie/DimKitchen
        //    //homie/DimKitchen/$homie			"3.0.1"
        //    //homie/DimKitchen/$name 			"Kitchen lights"
        //    //homie/DimKitchen/$localip			"0.0.0.0"
        //    //homie/DimKitchen/$mac				"00:00:00:00:00:00"
        //    //homie/DimKitchen/$fw/name			"BachelorPad"
        //    //homie/DimKitchen/$fw/version		"1.0.0"
        //    //homie/DimKitchen/$nodes			"lights,signal"
        //    //homie/DimKitchen/$implementation	"xComfort"
        //    //homie/DimKitchen/$stats/interval	"60"
        //    //homie/DimKitchen/$state			"ready"

        //    //                 NODE
        //    //homie/DimKitchen/lights
        //    //homie/DimKitchen/signal

        //    //homie/DimKitchen/lights/$name                 =   Lights
        //    //homie/DimKitchen/lights/$type                 =   Dimmer
        //    //homie/DimKitchen/lights/$properties           =   intensity

        //    //homie/DimKitchen/signal/$name                 =   "RSSI"
        //    //homie/DimKitchen/signal/$type                 =   "Signal"
        //    //homie/DimKitchen/signal/$properties           =   "strength"

        //    //                        PROPERTY
        //    //homie/DimKitchen/lights/intensity/$settable   =   "true"
        //    //homie/DimKitchen/lights/intensity/$unit       =   "%"
        //    //homie/DimKitchen/lights/intensity/$datatype   =   "integer"
        //    //homie/DimKitchen/lights/intensity/$format     =   "0:100"

        //    //homie/DimKitchen/signal/strength/$settable    =   "true"
        //    //homie/DimKitchen/signal/strength/$unit        =   "-mDb"
        //    //homie/DimKitchen/signal/strength/$datatype    =   "integer"
        //    //homie/DimKitchen/signal/strength/$format      =   "0:120"


        //    //string publishPath;
        //    switch (nodeName)
        //    {
        //        case "power":
        //            {
        //                return new Node { Name = nodeName, Properties=GetPropertiesForDeviceType(), Property=null  };
        //            }
        //        case "intensity":
        //            {
        //                return new PropertyOld { Name = nodeName, Settable = "true", Unit = "%", DataType = "boolean", Format = "0-100", PublishPath = publishPath };
        //            }
        //        case "jalousie":
        //            {
        //                return new PropertyOld { Name = nodeName, Settable = "true", DataType = "string", PublishPath = publishPath };
        //            }
        //        case "buttonstate":
        //            {
        //                return new PropertyOld { Name = nodeName, Settable = "false", DataType = "string", PublishPath = publishPath };
        //            }
        //        case "wheelposition":
        //            {
        //                return new PropertyOld { Name = nodeName, Settable = "false", DataType = "float", Format = "-15:3", PublishPath = publishPath };
        //            }
        //        case "temperature":
        //            {
        //                return new PropertyOld { Name = nodeName, Settable = "true", DataType = "boolean", PublishPath = publishPath };
        //            }
        //        case "humidity":
        //            {
        //                return new PropertyOld { Name = nodeName, Settable = "false", Unit = "%", DataType = "integer", Format = "0-100", PublishPath = publishPath };
        //            }
        //        case "battery":
        //            {
        //                return new PropertyOld { Name = nodeName, Settable = "false", DataType = "integer", Format = "0-10", PublishPath = publishPath };
        //            }
        //        case "signal":
        //            {
        //                return new PropertyOld { Name = nodeName, Settable = "false", Unit = "-dBm", DataType = "integer", Format = "0-120", PublishPath = publishPath };
        //            }
        //        default:
        //            {
        //                // This is the bare minimum required to move the data through the system so that it'll be
        //                // possible to handle those special devices in the next system...
        //                return new PropertyOld { Name = nodeName, Settable = "false", DataType = "string", PublishPath = publishPath };
        //            }
        //    }
        //}

        public static Property GetPropertyFromPropertyName(string propertyName)
        {
            string publishPath = "";
            switch (propertyName)
            {
                case "power":
                    {
                        return new Property { Name = propertyName, Settable = "true", DataType = "boolean", PublishPath = publishPath };
                    }
                case "intensity":
                    {
                        return new Property { Name = propertyName, Settable = "true", Unit = "%", DataType = "boolean", Format = "0-100", PublishPath = publishPath };
                    }
                case "jalousie":
                    {
                        return new Property { Name = propertyName, Settable = "true", DataType = "string", PublishPath = publishPath };
                    }
                case "buttonstate":
                    {
                        return new Property { Name = propertyName, Settable = "false", DataType = "string", PublishPath = publishPath };
                    }
                case "wheelposition":
                    {
                        return new Property { Name = propertyName, Settable = "false", DataType = "float", Format = "-15:3", PublishPath = publishPath };
                    }
                case "temperature":
                    {
                        return new Property { Name = propertyName, Settable = "true", DataType = "boolean", PublishPath = publishPath };
                    }
                case "humidity":
                    {
                        return new Property { Name = propertyName, Settable = "false", Unit = "%", DataType = "integer", Format = "0-100", PublishPath = publishPath };
                    }
                case "battery":
                    {
                        return new Property { Name = propertyName, Settable = "false", DataType = "integer", Format = "0-10", PublishPath = publishPath };
                    }
                case "signal":
                    {
                        return new Property { Name = propertyName, Settable = "false", Unit = "-dBm", DataType = "integer", Format = "0-120", PublishPath = publishPath };
                    }
                default:
                    {
                        // This is the bare minimum required to move the data through the system so that it'll be
                        // possible to handle those special devices in the next system...
                        return new Property { Name = propertyName, Settable = "false", DataType = "string", PublishPath = publishPath };
                    }
            }
        }
    #endregion

        }
    }
