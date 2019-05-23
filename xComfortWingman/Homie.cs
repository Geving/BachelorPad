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
        public static string GetStatsCPUtemp()
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
        public static string GetStatsCPUload()
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

         public class Device // Represents a Datapoint. Datapoints represents xComfort devices.
        {
            public string Homie { get; set; }  =  Program.Settings.HOMIE_HOMIE;                         //homie/DimKitchen/$homie			"3.0.1"
            public string Name  { get; set; }                                                           //homie/DimKitchen/$name 			"Kitchen lights"
            public string Localip { get; set; } = LocalIP;                                              //homie/DimKitchen/$localip			"0.0.0.0"
            public string Mac  { get; set; } = MAC;                                                     //homie/DimKitchen/$mac				"00:00:00:00:00:00"
            public string Fw_name  { get; set; } =   Program.Settings.HOMIE_FW_NAME;                    //homie/DimKitchen/$fw/name			"BachelorPad"
            public string Fw_version  { get; set; } =   Program.Settings.HOMIE_FW_VERSION;              //homie/DimKitchen/$fw/version		"1.0.0"
            public string Nodes  { get; set; }                                                          //homie/DimKitchen/$nodes			"lights,signal"
            public string Implementation  { get; set; } =  Program.Settings.HOMIE_IMPLEMENTATION;       //homie/DimKitchen/$implementation	"xComfort"
            public string Stats_interval  { get; set; } =   Program.Settings.HOMIE_STATS_INTERVAL;      //homie/DimKitchen/$stats/interval	"60"
            public string State  { get; set; }                                                          //homie/DimKitchen/$state			"ready"

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

        public static Device CreateDeviceFromDatapoint(Datapoint datapoint)
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
                case 5:  { return "roomcontroller1,battery,signal"; } //{ return "temperature,wheelposition,battery,signal"; }
                case 51: { return "roomcontroller2,battery,signal"; } // { return "temperature,wheelposition,humidity,battery,signal"; }
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
                    case "Dimmer": { node.Value = packet.MGW_RX_INFO_SHORT.ToString(); break; }
                    default: { node.Value = CI.GetDataFromPacket(packet.MGW_RX_DATA, packet.MGW_RX_DATA_TYPE, ""); break; }
                }
                MyLogger.DoLog($"Updating data for {device.Name}'s {node.Name}: {node.Value}...",4);
                await MQTT.SendMQTTMessageAsync($"{device.Name}/{node.PathName}/{node.PropertyList[0].PathName}", node.Value, true);
            }
        }

        public static async Task UpdateSingleProperty(string topic, Property property, string newDataValue)
        {
            MyLogger.DoLog($"Updating {topic} with new value: {newDataValue}",2);
            property.DataValue = newDataValue;
            await MQTT.SendMQTTMessageAsync(topic, newDataValue, true);
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
        }

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
