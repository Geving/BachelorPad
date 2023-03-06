using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace xComfortWingman
{
    public class HomeAssistant
    {
        public readonly static string HomeAssistantBaseTopic = Program.Settings.HOMEASSISTANT_BASETOPIC; //"HomeAssistantBaseTopic";
        public readonly static string AutoDiscoveryTopic = Program.Settings.HOMEASSISTANT_DISCOVERYTOPIC; //"homeassistant";
        public readonly static string AvailabilityTopic = Program.Settings.HOMEASSISTANT_AVAILABILITYTOPIC;

        public static List<Device> deviceList = new List<Device>();

        public static Device SetupNewDevice(Datapoint datapoint, string name = "")
        {
            if (Program.Settings.GENERAL_FILTER_DUPLICATE_DP)
            {
                foreach (Device device in deviceList)
                {
                    if (device.DP == datapoint.DP)
                    {
                        MyLogger.DoLog($"Datapoint " + device.DP + " already exists! Skipping...", true);
                        return device;
                    }
                }
            }
            Device newDevice;
            switch (datapoint.Type)
            {
                case 1:
                case 2:
                case 3: // Push button (single, double, quad)
                    {
                        newDevice = new Switch(datapoint);
                        break;
                    }
                case 5:
                case 23:
                case 51: // Room controller or temperature input
                    {
                        newDevice = new Sensor(datapoint);
                        break;
                    }
                case 19:
                case 22:
                case 20: // Binary inputs
                    {
                        newDevice = new BinarySensor(datapoint);
                        break;
                    }
                default:
                    {
                        newDevice = new Light(datapoint);
                        break;
                    }
            }
            //Common data

            //newDevice.Identifiers = $"{datapoint.DP}_{datapoint.Serial}";
            if (name != "") newDevice.Name = name;



            deviceList.Add(newDevice);
            return newDevice;
        }

        public static void Heartbeat(string payload = "online")
        {
            MyLogger.DoLog($"Sending Heartbeat...", true);
            MQTT.SendMQTTMessageAsync($"{AvailabilityTopic}", payload, true).Wait(new TimeSpan(0, 0, 0, 0, 50));
            MQTT.TimeOfLastHeartbeat = DateTime.Now.Ticks;
            //MyLogger.DoLog("OK", 3, true, 10);
        }

        public partial class Device
        {
            public int DP { get; set; }
            public string devtopic { get; set; }
            public string Config_topic { get; set; }
            //public string currentState { get; set; }
            public bool ReadOnly { get; set; } = true;
            public string devtype { get; set; }

            public string Name { get; set; }
            public string Manufacturer { get; set; } = "xComfort";
            public string Model { get; set; }

            //For the device
            //public string Device { get; set; }
            public string Connections { get; set; }
            public string Identifiers { get; set; }
            public string Sw_version { get; set; }
            public string Via_device { get; set; } = "xComfort2MQTT";

            //Common for all types
            public string Availability_topic { get; set; } = $"{AvailabilityTopic}";
            public string Payload_available { get; set; } = "online";
            public string Payload_not_available { get; set; } = "offline";
            public string Payload_off { get; set; } = "ON";
            public string Payload_on { get; set; } = "OFF";
            public int Qos { get; set; } = 0;
            public bool Retain { get; set; } = false;
            public string State_on { get; set; } = "ON";
            public string State_off { get; set; } = "OFF";
            public string State_topic { get; set; } = "UndefinedStateTopic";
            public string Value_template { get; set; }
            public string Command_topic { get; set; }

            //public string[] ConfigString { get; set; } = new string[1];
            public Dictionary<string, string> AutoConfigs { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, string> States = new Dictionary<string, string>();

        }

        public class Switch : Device
        {

            public string Icon { get; set; }
            public string Json_attributes_template { get; set; }
            public string Json_attributes_topic { get; set; }
            public bool Optimistic { get; set; }

            public Switch(Datapoint datapoint)
            {
                this.devtype = "Switch";
                this.devtopic = $"{HomeAssistantBaseTopic}/{devtype.ToLower()}/{SanitiseString(datapoint.Name)}";
                this.Config_topic = $"{AutoDiscoveryTopic}/{devtype.ToLower()}/{SanitiseString(datapoint.Name)}/config";
                //Return based solely on the datapoint provided
                this.Name = datapoint.PrettyName; //SanitiseString(datapoint.Name);
                this.Model = GetModelNameForDeviceType(datapoint.Type);
                //this.Availability_topic = $"{devtopic}/{Availability_topic}";
                this.DP = datapoint.DP;
                //this.Identifiers = datapoint.DP.ToString() + datapoint.Serial.ToString();
                this.Identifiers = $"{datapoint.DP}_{datapoint.Serial}";
                this.State_topic = $"{devtopic}/state";
                this.Command_topic = $"{devtopic}/set";

                this.AutoConfigs.Add(this.Config_topic,
                    $"{{ " +
                    $"\"name\": \"{this.Name}\", " +
                    $"\"unique_id\": \"{this.Identifiers}\", " +
                    $"\"command_topic\": \"{this.Command_topic}\", " +
                    $"\"state_topic\": \"{this.State_topic}\", " +
                    $"\"value_template\": \"{{{{ value_json.state }}}}\", " +
                    $"\"device\": {{" +
                                $"  \"identifiers\": [" +
                                $"    \"{this.Identifiers}\" " +
                                $"  ], " +
                                $"  \"name\": \"{this.Name}\", " +
                                $"  \"sw_version\": \"{this.Sw_version}\", " +
                                $"  \"model\": \"{this.Model}\", " +
                                $"  \"manufacturer\": \"{this.Manufacturer}\"" +
                                $"}}," +
                    $"\"availability_topic\": \"{this.Availability_topic}\"," +
                    $"\"payload_available\": \"{this.Payload_available}\"," +
                    $"\"payload_not_available\": \"{this.Payload_not_available}\"" +
                    $"}}");

                //,"payload_available":"Online","payload_not_available":"Offline"
            }

        }

        public partial class SensorBase : Device
        {
            public int Expire_after { get; set; }
            public bool Force_update { get; set; }

        }

        public class BinarySensor : SensorBase
        {
            //private static string[] BinarySensorClass = { "None", "battery", "battery_charging", "cold", "connectivity", "door", "garage_door", "gas", "heat", "light", "lock", "moisture", "motion", "moving", "occupancy", "opening", "plug", "power", "presence", "problem", "safety", "smoke", "sound", "vibration", "window" };
            public string Device_class { get; set; }// = BinarySensorClass[0];
            public int off_delay { get; set; }
            public string unique_id { get; set; }

            public BinarySensor(Datapoint datapoint)
            {
                this.devtype = "Binary_Sensor";
                this.devtopic = $"{HomeAssistantBaseTopic}/{devtype.ToLower()}/{SanitiseString(datapoint.Name)}";
                this.Config_topic = $"{AutoDiscoveryTopic}/{devtype.ToLower()}/{SanitiseString(datapoint.Name)}/config";
                //Return based solely on the datapoint provided
                this.Name = datapoint.PrettyName; //SanitiseString(datapoint.Name);
                this.Model = GetModelNameForDeviceType(datapoint.Type);
                //this.Availability_topic = $"{devtopic}/{Availability_topic}";
                this.DP = datapoint.DP;
                //this.Identifiers = datapoint.DP.ToString() + datapoint.Serial.ToString() + datapoint.Channel.ToString();
                this.Identifiers = $"{datapoint.DP}_{datapoint.Serial}";
                this.off_delay = 0;
                this.Device_class = GetSensorTypeForDeviceType(datapoint.Type);
                this.Expire_after = GetExpiryForDeviceType(datapoint.Type);
                this.State_topic = $"{devtopic}/state";

                this.AutoConfigs.Add(this.Config_topic, // + "_State",
                    $"{{" +
                    $"\"name\": \"{this.Name}\", " +
                    $"\"unique_id\": \"{this.Identifiers}\", " +
                    $"\"state_topic\": \"{this.State_topic}\", " +
                    $"\"json_attributes_topic\": \"{this.State_topic}\", " +
                     $"\"device\": {{" +
                                $"  \"identifiers\": [" +
                                $"    \"{this.Identifiers}\" " +
                                $"  ], " +
                                $"  \"name\": \"{this.Name}\", " +
                                $"  \"sw_version\": \"{this.Sw_version}\", " +
                                $"  \"model\": \"{this.Model}\", " +
                                $"  \"manufacturer\": \"{this.Manufacturer}\"" +
                                $"}}," +
                    $"\"availability_topic\": \"{this.Availability_topic}\"," +
                    $"\"payload_available\": \"{this.Payload_available}\"," +
                    $"\"payload_not_available\": \"{this.Payload_not_available}\"," +
                    $"\"value_template\": \"{{{{ value_json.state }}}}\" " +
                    $"}}");
            }
        }

        public class Sensor : SensorBase
        {
            //private static string[] SensorClass = { "None", "battery", "humidity", "illuminance", "signal_strength", "temperature", "power", "pressure", "timestamp" };
            public string Device_class { get; set; } = "None";
            public string Unit_of_measurement { get; set; } = "";
            public string Icon { get; set; } = "";

            public Sensor(Datapoint datapoint)
            {
                this.devtype = "Sensor";
                this.devtopic = $"{HomeAssistantBaseTopic}/{devtype.ToLower()}/{SanitiseString(datapoint.Name)}";
                this.Config_topic = $"{AutoDiscoveryTopic}/{devtype.ToLower()}/{SanitiseString(datapoint.Name)}/config";
                //Return based solely on the datapoint provided
                this.Name = datapoint.PrettyName; //SanitiseString(datapoint.Name);
                this.Model = GetModelNameForDeviceType(datapoint.Type);
                //this.Availability_topic = $"{devtopic}/{Availability_topic}";
                this.State_topic = $"{devtopic}/state";
                this.DP = datapoint.DP;
                //this.Identifiers = datapoint.DP.ToString() + datapoint.Serial.ToString();
                this.Identifiers = $"{datapoint.DP}_{datapoint.Serial}";
                this.Device_class = GetSensorTypeForDeviceType(datapoint.Type);
                this.Expire_after = GetExpiryForDeviceType(datapoint.Type);
                this.Unit_of_measurement = GetSensorDataUnitForDeviceType(datapoint.Type);

                switch (datapoint.Type)
                {
                    case 23: //Temperature sensor
                        {

                            this.AutoConfigs.Add(this.Config_topic + "_TempA",
                                $"{{" +
                                $"\"name\": \"{this.Name}_Temperature_A\", " +
                                $"\"unique_id\": \"{this.Identifiers}_A\", " +
                                $"\"state_topic\": \"{this.State_topic}\", " +
                                $"\"device_class\": \"temperature\", " +
                                $"\"unit_of_measurement\": \"{this.Unit_of_measurement}\", " +
                                $"\"json_attributes_topic\": \"{this.State_topic}\", " +
                                $"\"value_template\": \"{{{{ value_json.temperature_a }}}}\", " +
                                $"\"device\": {{" +
                                $"  \"identifiers\": [" +
                                $"    \"{this.Identifiers}\" " +
                                $"  ], " +
                                $"  \"name\": \"{this.Name}\", " +
                                $"  \"sw_version\": \"{this.Sw_version}\", " +
                                $"  \"model\": \"{this.Model}\", " +
                                $"  \"manufacturer\": \"{this.Manufacturer}\"" +
                                $"}}," +
                    $"\"availability_topic\": \"{this.Availability_topic}\"," +
                    $"\"payload_available\": \"{this.Payload_available}\"," +
                    $"\"payload_not_available\": \"{this.Payload_not_available}\"" +
                                $"}}");
                            this.AutoConfigs.Add(this.Config_topic + "_TempB",
                                $"{{" +
                                $"\"name\": \"{this.Name}_Temperature_B\", " +
                                $"\"unique_id\": \"{this.Identifiers}_B\", " +
                                $"\"state_topic\": \"{this.State_topic}\", " +
                                $"\"device_class\": \"temperature\", " +
                                $"\"json_attributes_topic\": \"{this.State_topic}\", " +
                                $"\"value_template\": \"{{{{ value_json.temperature_b }}}}\", " +
                                $"\"device\": {{" +
                                $"  \"identifiers\": [" +
                                $"    \"{this.Identifiers}\" " +
                                $"  ], " +
                                $"  \"name\": \"{this.Name}\", " +
                                $"  \"sw_version\": \"{this.Sw_version}\", " +
                                $"  \"model\": \"{this.Model}\", " +
                                $"  \"manufacturer\": \"{this.Manufacturer}\"" +
                                $"}}," +
                    $"\"availability_topic\": \"{this.Availability_topic}\"," +
                    $"\"payload_available\": \"{this.Payload_available}\"," +
                    $"\"payload_not_available\": \"{this.Payload_not_available}\"" +
                                $"}}");
                            break;
                        }
                    case 5: //Room controller
                        {
                            this.AutoConfigs.Add(this.Config_topic, // + "_Temp",
                                $"{{" +
                                $"\"name\": \"{this.Name}_Temperature\", " +
                                $"\"unique_id\": \"{this.Identifiers}_T\", " +
                                $"\"state_topic\": \"{this.State_topic}\", " +
                                $"\"device_class\": \"temperature\", " +
                                $"\"unit_of_measurement\": \"{this.Unit_of_measurement}\", " +
                                $"\"json_attributes_topic\": \"{this.State_topic}\", " +
                                $"\"value_template\": \"{{{{ value_json.temperature }}}}\", " +
                                $"\"device\": {{" +
                                $"  \"identifiers\": [" +
                                $"    \"{this.Identifiers}\" " +
                                $"  ], " +
                                $"  \"name\": \"{this.Name}\", " +
                                $"  \"sw_version\": \"{this.Sw_version}\", " +
                                $"  \"model\": \"{this.Model}\", " +
                                $"  \"manufacturer\": \"{this.Manufacturer}\"" +
                                $"}}," +
                    $"\"availability_topic\": \"{this.Availability_topic}\"," +
                    $"\"payload_available\": \"{this.Payload_available}\"," +
                    $"\"payload_not_available\": \"{this.Payload_not_available}\"" +
                                $"}}");


                            break;
                        }
                    case 51: //Room controller w/humidity
                        {
                            this.AutoConfigs.Add(this.Config_topic, // + "_Temp",
                                $"{{" +
                                $"\"name\": \"{this.Name}_Temperature\", " +
                                $"\"unique_id\": \"{this.Identifiers}_T\", " +
                                $"\"state_topic\": \"{this.State_topic}\", " +
                                $"\"schema\": \"json\", " +
                                $"\"json_attributes_topic\": \"{this.State_topic}\", " +
                                $"\"value_template\": \"{{{{ value_json.temperature }}}}\", " +
                                $"\"device\": {{" +
                                $"  \"identifiers\": [" +
                                $"    \"{this.Identifiers}\" " +
                                $"  ], " +
                                $"  \"name\": \"{this.Name}\", " +
                                $"  \"sw_version\": \"{this.Sw_version}\", " +
                                $"  \"model\": \"{this.Model}\", " +
                                $"  \"manufacturer\": \"{this.Manufacturer}\"" +
                                $"}}," +
                    $"\"availability_topic\": \"{this.Availability_topic}\"," +
                    $"\"payload_available\": \"{this.Payload_available}\"," +
                    $"\"payload_not_available\": \"{this.Payload_not_available}\"" +
                                $"}}");
                            break;
                        }
                    default:
                        {
                            this.AutoConfigs.Add(this.Config_topic, // + "_State",
                                $"{{" +
                                $"\"name\": \"{this.Name}\", " +
                                $"\"unique_id\": \"{this.Identifiers}\", " +
                                $"\"state_topic\": \"{this.State_topic}\", " +
                                $"\"json_attributes_topic\": \"{this.State_topic}\", " +
                                $"\"value_template\": \"{{{{ value_json.state }}}}\", " +
                                $"\"device\": {{" +
                                $"  \"identifiers\": [" +
                                $"    \"{this.Identifiers}\" " +
                                $"  ], " +
                                $"  \"name\": \"{this.Name}\", " +
                                $"  \"sw_version\": \"{this.Sw_version}\", " +
                                $"  \"model\": \"{this.Model}\", " +
                                $"  \"manufacturer\": \"{this.Manufacturer}\"" +
                                $"}}," +
                    $"\"availability_topic\": \"{this.Availability_topic}\"," +
                    $"\"payload_available\": \"{this.Payload_available}\"," +
                    $"\"payload_not_available\": \"{this.Payload_not_available}\"" +
                                $"}}");
                            break;
                        }

                }

            }
        }

        public class Light : Device
        {
            public string Brightness_command_topic { get; set; }
            public int Brightness_scale { get; set; }
            public string Brightness_state_topic { get; set; }
            public string Brightness_value_template { get; set; }
            public string color_temp_command_template { get; set; }
            public string color_temp_command_topic { get; set; }
            public string color_temp_state_topic { get; set; }
            public string color_temp_value_template { get; set; }
            public string effect_command_topic { get; set; }
            public string[] effect_list { get; set; }
            public string effect_state_topic { get; set; }
            public string effect_value_template { get; set; }
            public string hs_command_topic { get; set; }
            public string hs_state_topic { get; set; }
            public string hs_value_template { get; set; }
            public string on_command_type { get; set; }
            public bool optimistic { get; set; }
            public string rgb_command_template { get; set; }
            public string rgb_command_topic { get; set; }
            public string rgb_state_topic { get; set; }
            public string rgb_value_template { get; set; }
            public string schema { get; set; }
            public string state_value_template { get; set; }
            public string white_value_command_topic { get; set; }
            public string white_value_scale { get; set; }
            public string white_value_state_topic { get; set; }
            public string white_value_template { get; set; }
            public string xy_command_topic { get; set; }
            public string xy_state_topic { get; set; }
            public string xy_value_template { get; set; }

            public Light(Datapoint datapoint)
            {
                this.devtype = "Light";
                this.ReadOnly = false;
                this.devtopic = $"{HomeAssistantBaseTopic}/{devtype.ToLower()}/{SanitiseString(datapoint.Name)}";
                this.Config_topic = $"{AutoDiscoveryTopic}/{devtype.ToLower()}/{SanitiseString(datapoint.Name)}/config";
                //Return based solely on the datapoint provided
                this.Name = datapoint.PrettyName; //SanitiseString(datapoint.Name);
                this.Model = GetModelNameForDeviceType(datapoint.Type);
                //this.Availability_topic = $"{devtopic}/{Availability_topic}";
                this.DP = datapoint.DP;
                //this.Identifiers = datapoint.DP.ToString() + datapoint.Serial.ToString();
                this.Identifiers = $"{datapoint.DP}_{datapoint.Serial}";
                this.Brightness_scale = 100;
                this.Brightness_command_topic = $"{devtopic}/set";
                this.Brightness_state_topic = $"{devtopic}/brightness";
                this.State_topic = $"{devtopic}/state"; //this.Brightness_state_topic; // $"{devtopic}/state";
                this.Command_topic = $"{devtopic}/set";

                this.AutoConfigs.Add(this.Config_topic, // + "_Brightness",
                    $"{{" +
                    $"\"name\": \"{this.Name}\", " +
                    $"\"unique_id\": \"{this.Identifiers}\", " +
                    $"\"state_topic\": \"{this.State_topic}\", " +
                    $"\"command_topic\": \"{this.Command_topic}\", " +
                    //$"\"brightness_state_topic\": \"{this.Brightness_state_topic}\", " +
                    //$"\"brightness_command_topic\": \"{this.Brightness_command_topic}\", " +
                    $"\"brightness_scale\": 100, " +
                    $"\"brightness\": true, " +
                    //$"\"optimistic\": true, " +
                    //$"\"on_command_type\": \"brightness\", " +
                    $"\"schema\": \"json\", " +
                    //$"\"brightness_value_template\": \"{{{{ brightness_state_json.Brightness }}}}\", " +
                    //$"\"state_value_template\": \"{{{{ state_value_json.Brightness }}}}\", " +
                    $"\"json_attributes_topic\": \"{this.State_topic}\", " +
                    $"\"value_template\": \"{{{{ value_json.brightness }}}}\", " +
                    $"\"device\": {{" +
                                $"  \"identifiers\": [" +
                                $"    \"{this.Identifiers}\" " +
                                $"  ], " +
                                $"  \"name\": \"{this.Name}\", " +
                                $"  \"sw_version\": \"{this.Sw_version}\", " +
                                $"  \"model\": \"{this.Model}\", " +
                                $"  \"manufacturer\": \"{this.Manufacturer}\"" +
                                $"}}," +
                    $"\"availability_topic\": \"{this.Availability_topic}\"," +
                    $"\"payload_available\": \"{this.Payload_available}\"," +
                    $"\"payload_not_available\": \"{this.Payload_not_available}\"" +
                    $"}}");
            }
        }

        public class Trigger : Device
        {
            private static string[] types = { "button_short_press", "button_short_release", "button_long_press", "button_long_release", "button_double_press", "button_triple_press", "button_quadruple_press", "button_quintuple_press" };
            private static string[] subtypes = { "turn_on", "turn_off", "button_1", "button_2", "button_3", "button_4", "button_5", "button_6" };

            public string automation_type = "trigger";
            public string payload;
            public string topic;
            public string type = types[0];
            public string subtype = subtypes[0];

            public Trigger(string automation_type, string payload, string topic, string type, string subtype)
            {
                this.devtype = "trigger";
                this.automation_type = automation_type;
                this.payload = payload;
                this.topic = topic;
                this.type = type;
                this.subtype = subtype;
            }
        }

        public class Cover : Device
        {
            public int position_closed;
            public int position_open;
            public string set_position_template;
            public string set_position_topic;
            public string state_closed;
            public string state_closing;
            public string state_open;
            public string state_opening;
            public int tilt_closed_value;
            public string tilt_command_topic;
            public bool tilt_invert_state;
            public int tilt_max;
            public int tilt_min;
            public int tilt_opened_value;
            public bool tilt_optimistic;
            public string tilt_status_template;
            public string tilt_status_topic;

            public Cover(string devtype, int position_closed, int position_open, string set_position_template, string set_position_topic, string state_closed, string state_closing, string state_open, string state_opening, int tilt_closed_value, string tilt_command_topic, bool tilt_invert_state, int tilt_max, int tilt_min, int tilt_opened_value, bool tilt_optimistic, string tilt_status_template, string tilt_status_topic)
            {
                this.devtype = "Cover";
                this.position_closed = position_closed;
                this.position_open = position_open;
                this.set_position_template = set_position_template;
                this.set_position_topic = set_position_topic;
                this.state_closed = state_closed;
                this.state_closing = state_closing;
                this.state_open = state_open;
                this.state_opening = state_opening;
                this.tilt_closed_value = tilt_closed_value;
                this.tilt_command_topic = tilt_command_topic;
                this.tilt_invert_state = tilt_invert_state;
                this.tilt_max = tilt_max;
                this.tilt_min = tilt_min;
                this.tilt_opened_value = tilt_opened_value;
                this.tilt_optimistic = tilt_optimistic;
                this.tilt_status_template = tilt_status_template;
                this.tilt_status_topic = tilt_status_topic;
            }
        }

        public class Lock : Device
        {
            //public string devtype = "Lock";
            public string payload_lock = "LOCK";
            public string payload_unlock = "UNLOCK";
            public string state_locked = "LOCKED";
            public string state_unlocked = "UNLOCKED";

        }

        public class HVAC : Device
        {
            //public string devtype = "HVAC";
            public string action_template;
            public string action_topic;
            public string aux_command_topic;
            public string aux_state_template;
            public string aux_state_topic;
            public string away_mode_command_topic;
            public string away_mode_state_template;
            public string away_mode_state_topic;
            public string current_temperature_template;
            public string current_temperature_topic;
            public string fan_mode_command_topic;
            public string fan_mode_state_template;
            public string fan_mode_state_topic;
            public string fan_modes;
            public string hold_command_topic;
            public string hold_state_template;
            public string hold_state_topic;
            public string hold_modes;
            public int initial;
            public float max_temp;
            public float min_temp;
            public string mode_command_topic;
            public string mode_state_template;
            public string mode_state_topic;
            public string modes;
            public string power_command_topic;
            public float precision;
            public bool send_if_off;
            public string swing_mode_command_topic;
            public string swing_mode_state_template;
            public string swing_mode_state_topic;
            public string swing_modes;
            public string temperature_command_topic;
            public string temperature_high_command_topic;
            public string temperature_high_state_template;
            public string temperature_high_state_topic;
            public string temperature_low_command_topic;
            public string temperature_low_state_template;
            public string temperature_low_state_topic;
            public string temperature_state_template;
            public string temperature_state_topic;
            public string temperature_unit;
            public float temp_step;

            public HVAC(string devtype, string action_template, string action_topic, string aux_command_topic, string aux_state_template, string aux_state_topic, string away_mode_command_topic, string away_mode_state_template, string away_mode_state_topic, string current_temperature_template, string current_temperature_topic, string fan_mode_command_topic, string fan_mode_state_template, string fan_mode_state_topic, string fan_modes, string hold_command_topic, string hold_state_template, string hold_state_topic, string hold_modes, int initial, float max_temp, float min_temp, string mode_command_topic, string mode_state_template, string mode_state_topic, string modes, string power_command_topic, float precision, bool send_if_off, string swing_mode_command_topic, string swing_mode_state_template, string swing_mode_state_topic, string swing_modes, string temperature_command_topic, string temperature_high_command_topic, string temperature_high_state_template, string temperature_high_state_topic, string temperature_low_command_topic, string temperature_low_state_template, string temperature_low_state_topic, string temperature_state_template, string temperature_state_topic, string temperature_unit, float temp_step)
            {
                this.devtype = devtype;
                this.action_template = action_template;
                this.action_topic = action_topic;
                this.aux_command_topic = aux_command_topic;
                this.aux_state_template = aux_state_template;
                this.aux_state_topic = aux_state_topic;
                this.away_mode_command_topic = away_mode_command_topic;
                this.away_mode_state_template = away_mode_state_template;
                this.away_mode_state_topic = away_mode_state_topic;
                this.current_temperature_template = current_temperature_template;
                this.current_temperature_topic = current_temperature_topic;
                this.fan_mode_command_topic = fan_mode_command_topic;
                this.fan_mode_state_template = fan_mode_state_template;
                this.fan_mode_state_topic = fan_mode_state_topic;
                this.fan_modes = fan_modes;
                this.hold_command_topic = hold_command_topic;
                this.hold_state_template = hold_state_template;
                this.hold_state_topic = hold_state_topic;
                this.hold_modes = hold_modes;
                this.initial = initial;
                this.max_temp = max_temp;
                this.min_temp = min_temp;
                this.mode_command_topic = mode_command_topic;
                this.mode_state_template = mode_state_template;
                this.mode_state_topic = mode_state_topic;
                this.modes = modes;
                this.power_command_topic = power_command_topic;
                this.precision = precision;
                this.send_if_off = send_if_off;
                this.swing_mode_command_topic = swing_mode_command_topic;
                this.swing_mode_state_template = swing_mode_state_template;
                this.swing_mode_state_topic = swing_mode_state_topic;
                this.swing_modes = swing_modes;
                this.temperature_command_topic = temperature_command_topic;
                this.temperature_high_command_topic = temperature_high_command_topic;
                this.temperature_high_state_template = temperature_high_state_template;
                this.temperature_high_state_topic = temperature_high_state_topic;
                this.temperature_low_command_topic = temperature_low_command_topic;
                this.temperature_low_state_template = temperature_low_state_template;
                this.temperature_low_state_topic = temperature_low_state_topic;
                this.temperature_state_template = temperature_state_template;
                this.temperature_state_topic = temperature_state_topic;
                this.temperature_unit = temperature_unit;
                this.temp_step = temp_step;
            }
        }

        public static string GetModelNameForDeviceType(int type)
        {
            switch (type)
            {
                case 1: //Push-button (Single)
                    return "Push-button (Single)";
                case 2: //Push-button (Dual)
                    return "Push-button (Dual)";
                case 3: //Push-button (Quad)
                    return "Push-button (Quad)";
                case 4: //Room Controller ( /w Switch)
                    return "Room Controller ( /w Switch)";
                case 5: //Room Controller ( /w Switch)
                    return "Room Controller ( /w Switch)";
                case 6: //Switching Actuator
                    return "Switching Actuator";
                case 7:
                case 17: //Dimming Actuator
                    return "Dimming Actuator";
                case 8: //Jalousie Actuator
                    return "Jalousie Actuator";
                case 9: //Binary Input, 230V
                    return "Binary Input, 230V";
                case 10: //Binary Input, 230V
                    return "Binary Input, 230V";
                case 11: //Binary Input, 230V
                    return "Binary Input, 230V";
                case 12: //Binary Input, 230V
                    return "Binary Input, 230V";
                case 13: //Binary Input, 230V
                    return "Binary Input, 230V";
                case 14: //Binary Input, Battery
                    return "Binary Input, Battery";
                case 15: //Binary Input, Battery
                    return "Binary Input, Battery";
                case 16: //Binary Input, Battery
                    return "Binary Input, Battery";
                //case 7: //Binary Input, Battery
                //return "Binary Input, Battery";
                case 18: //Remote Control 12 Channel (old design)
                    return "Remote Control 12 Channel (old design)";
                case 19: //Home-Manager
                    return "Home-Manager";
                case 20: //Temperature Input
                    return "Temperature Input";
                case 21: //Temperature Input
                    return "Temperature Input";
                case 22: //Analog Input
                    return "Analog Input";
                case 23: //Analog Input
                    return "Analog Input";
                case 24: //Analog Input
                    return "Analog Input";
                case 25: //Analog Input
                    return "Analog Input";
                case 26: //Analog Input
                    return "Analog Input";
                case 27: //Analog Actuator
                    return "Analog Actuator";
                case 28: //Room-Manager
                    return "Room-Manager";
                case 29: //Jalousie Actuator with Security
                    return "Jalousie Actuator with Security";
                case 30: //Communication Interface
                    return "Communication Interface";
                case 31: //Motion Detector
                    return "Motion Detector";
                case 32: //Remote Control 2 Channel small
                    return "Remote Control 2 Channel small";
                case 33: //Remote Control 12 Channel
                    return "Remote Control 12 Channel";
                case 34: //Remote Control 12 Channel w/ display
                    return "Remote Control 12 Channel w/ display";
                case 35: //Room Controller w/ Switch/Humidity
                    return "Room Controller w/ Switch/Humidity";
                case 36: //Room Controller w/ Switch/Humidity
                    return "Room Controller w/ Switch/Humidity";
                case 37: //Room Controller w/ Switch/Humidity
                    return "Room Controller w/ Switch/Humidity";
                case 38: //Room Controller w/ Switch/Humidity
                    return "Room Controller w/ Switch/Humidity";
                case 39: //Router (no communication possible, just ignore it)
                    return "Router (no communication possible, just ignore it)";
                case 40: //Impulse Input
                    return "Impulse Input";
                case 41: //EMS
                    return "EMS";
                case 42: //EMS
                    return "EMS";
                case 43: //EMS
                    return "EMS";
                case 44: //EMS
                    return "EMS";
                case 45: //E-Raditor Actuator
                    return "E-Raditor Actuator";
                case 46: //Remote Control Alarm Pushbutton
                    return "Remote Control Alarm Pushbutton";
                case 47: //BOSCOS (Bed/Chair Occupancy Sensor)
                    return "BOSCOS (Bed/Chair Occupancy Sensor)";
                case 48: //MEP
                    return "MEP";
                case 49: //MEP
                    return "MEP";
                case 50: //MEP
                    return "MEP";
                case 51: //MEP
                    return "MEP";
                case 52: //MEP
                    return "MEP";
                case 53: //MEP
                    return "MEP";
                case 54: //HRV
                    return "HRV";
                case 55: //Rosetta Sensor
                    return "Rosetta Sensor";
                case 56: //Rosetta Sensor
                    return "Rosetta Sensor";
                case 57: //Rosetta Sensor
                    return "Rosetta Sensor";
                case 58: //Rosetta Sensor
                    return "Rosetta Sensor";
                case 59: //Rosetta Router
                    return "Rosetta Router";
                case 60: //Multi Channel Heating Actuator
                    return "Multi Channel Heating Actuator";
                case 61: //Multi Channel Heating Actuator
                    return "Multi Channel Heating Actuator";
                case 62: //Multi Channel Heating Actuator
                    return "Multi Channel Heating Actuator";
                case 63: //Communication Interface USB
                    return "Communication Interface USB";
                case 64: //Switching Actuator New Generation
                    return "Switching Actuator New Generation";
                case 65: //Switching Actuator New Generation
                    return "Switching Actuator New Generation";
                case 66: //Switching Actuator New Generation
                    return "Switching Actuator New Generation";
                case 67: //Switching Actuator New Generation
                    return "Switching Actuator New Generation";
                case 68: //Switching Actuator New Generation
                    return "Switching Actuator New Generation";
                case 69: //Switching Actuator New Generation
                    return "Switching Actuator New Generation";
                case 70: //Router New Generation
                    return "Router New Generation";
                default:
                    return "Unknown";
            }
        }

        public static string GetSensorTypeForDeviceType(int type)
        {
            // Legal values:  "None", "battery", "humidity", "illuminance", "signal_strength", "temperature", "power", "pressure", "timestamp"
            // For Binary type: "None", "battery", "battery_charging", "cold", "connectivity", "door", "garage_door", "gas", "heat", "light", "lock", "moisture", "motion", "moving", "occupancy", "opening", "plug", "power", "presence", "problem", "safety", "smoke", "sound", "vibration", "window"
            switch (type)
            {
                case 4: //Room Controller ( /w Switch)
                case 5: //Room Controller ( /w Switch)
                    return "temperature";
                case 9: //Binary Input, 230V
                case 10: //Binary Input, 230V
                case 11: //Binary Input, 230V
                case 12: //Binary Input, 230V
                case 13: //Binary Input, 230V
                case 14: //Binary Input, Battery
                case 15: //Binary Input, Battery
                case 16: //Binary Input, Battery
                case 17: //Binary Input, Battery
                    return "power";
                case 20: //Temperature Input
                case 21: //Temperature Input
                    return "temperature";
                case 22: //Analog Input
                case 23: //Analog Input
                case 24: //Analog Input
                case 25: //Analog Input
                case 26: //Analog Input
                    return "None";
                case 31: //Motion Detector
                    return "motion";
                case 35: //Room Controller w/ Switch/Humidity
                case 36: //Room Controller w/ Switch/Humidity
                case 37: //Room Controller w/ Switch/Humidity
                case 38: //Room Controller w/ Switch/Humidity
                    return "humidity";
                case 40: //Impulse Input
                    return "motion";
                default:
                    return "None";
            }
        }

        public static int GetExpiryForDeviceType(int type)
        {
            return 3600;
        }

        static string GetSensorDataUnitForDeviceType(int type)
        {
            switch (type)
            {
                case 4: //Room Controller ( /w Switch)
                case 5: //Room Controller ( /w Switch)
                    return "C";
                case 9: //Binary Input, 230V
                case 10: //Binary Input, 230V
                case 11: //Binary Input, 230V
                case 12: //Binary Input, 230V
                case 13: //Binary Input, 230V
                case 14: //Binary Input, Battery
                case 15: //Binary Input, Battery
                case 16: //Binary Input, Battery
                case 17: //Binary Input, Battery
                    return "power";
                case 20: //Temperature Input
                case 21: //Temperature Input
                    return "C";
                case 22: //Analog Input
                case 23: //Analog Input
                case 24: //Analog Input
                case 25: //Analog Input
                case 26: //Analog Input
                    return "None";
                case 31: //Motion Detector
                    return "motion";
                case 35: //Room Controller w/ Switch/Humidity
                case 36: //Room Controller w/ Switch/Humidity
                case 37: //Room Controller w/ Switch/Humidity
                case 38: //Room Controller w/ Switch/Humidity
                    return "humidity";
                case 40: //Impulse Input
                    return "motion";
                default:
                    return "None";
            }
        }

        public static string SanitiseString(string str)
        {
            return System.Text.RegularExpressions.Regex.Replace(str, "[^a-zA-Z0-9]+", "", System.Text.RegularExpressions.RegexOptions.Compiled);
        }

        public static async Task UpdateDeviceData(Protocol.PT_RX.Packet packet)
        {
            Datapoint datapoint = CI.datapoints.Find(x => x.DP == packet.MGW_RX_DATAPOINT);
            Device device = deviceList.Find(d => d.DP == packet.MGW_RX_DATAPOINT);
            datapoint.LatestDataValues = packet;
            datapoint.LastUpdate = DateTime.Now;
            //string newData = CI.GetDataFromPacket(packet.MGW_RX_DATA, packet.MGW_RX_DATA_TYPE, "");
            device.States.Clear();
            device.States.Add("RSSI", packet.MGW_RX_RSSI.ToString());


            switch (packet.MGW_RX_DATA_TYPE)
            {
                case 0x00: //MGW_RDT_NO_DATA - No data
                    {
                        switch (device.devtype)
                        {
                            case "Light":
                                {
                                    device.States.Add("state", (packet.MGW_RX_INFO_SHORT > 0x02 ? "ON" : "OFF"));
                                    if (Program.Settings.HOMEASSISTANT_USE255ASMAX)
                                    {
                                        double newVal = packet.MGW_RX_INFO_SHORT * 2.55;
                                        device.States.Add("brightness", ((int)newVal).ToString());
                                    }
                                    else
                                    {
                                        device.States.Add("brightness", packet.MGW_RX_INFO_SHORT.ToString());
                                    }
                                    break;
                                }
                            case "Switch":
                                {
                                    string s = "UNKNOWN";
                                    switch (packet.MGW_RX_INFO_SHORT)
                                    {
                                        case 0x00: { s = "OFF"; break; }
                                        case 0x01: { s = "ON"; break; }
                                        case 0x02: { s = "UP"; break; }
                                        case 0x03: { s = "DOWN"; break; }
                                        default: { s = packet.MGW_RX_INFO_SHORT.ToString(); break; }
                                    }
                                    device.States.Add("state", s);
                                    device.States.Add("battery", packet.MGW_RX_BATTERY.ToString());
                                    break;
                                }
                            //case "Sensor":
                            //    {
                            //        status = $"\"State\":\"UNKNOWN\", ";
                            //        break;
                            //    }
                            //case "Binary_Sensor":
                            //    {
                            //        status = $"\"State\":\"UNKNOWN\", ";
                            //        break;
                            //    }
                            default:
                                {
                                    device.States.Add("battery", packet.MGW_RX_BATTERY.ToString());
                                    device.States.Add("state", packet.MGW_RX_INFO_SHORT.ToString());
                                    device.States.Add("MsgType", Protocol.PT_RX.MGW_RX_MSG_TYPE.GetTechnicalNameFromByte(packet.MGW_RX_MSG_TYPE));
                                    device.States.Add("DataType", Protocol.PT_RX.MGW_RX_DATA_TYPE.GetNameFromByte(packet.MGW_RX_DATA_TYPE));
                                    break;
                                }
                        }
                        break;
                    }
                case 0x01: //MGW_RDT_PERCENT - Dimmer value
                    {
                        if (Program.Settings.HOMEASSISTANT_USE255ASMAX)
                        {
                            double newVal = packet.MGW_RX_INFO_SHORT * 2.55;
                            device.States.Add("brightness", ((int)newVal).ToString());
                        }
                        else
                        {
                            device.States.Add("brightness", packet.MGW_RX_INFO_SHORT.ToString());
                        }
                        break;
                    }
                case 0x17: //MGW_RDT_RC_DATA - Room Controller data
                    {
                        device.States.Add("battery", packet.MGW_RX_BATTERY.ToString());
                        string mixedData = CI.GetDataFromPacket(packet.MGW_RX_DATA, packet.MGW_RX_DATA_TYPE, "");
                        device.States.Add("temperature", mixedData.Split(';')[1]);
                        device.States.Add("wheel", mixedData.Split(';')[0]);
                        break;
                    }
                default:
                    {
                        device.States.Add("battery", packet.MGW_RX_BATTERY.ToString());
                        device.States.Add("state", packet.MGW_RX_INFO_SHORT.ToString());
                        device.States.Add("MsgType", Protocol.PT_RX.MGW_RX_MSG_TYPE.GetTechnicalNameFromByte(packet.MGW_RX_MSG_TYPE));
                        device.States.Add("DataType", Protocol.PT_RX.MGW_RX_DATA_TYPE.GetNameFromByte(packet.MGW_RX_DATA_TYPE));
                        break;
                    }
            }


            string finalState = "{";
            foreach (KeyValuePair<string, string> keyValuePair in device.States)
            {
                if (keyValuePair.Key == "brightness")
                {
                    finalState += $"\"{keyValuePair.Key}\": {keyValuePair.Value}, ";
                }
                else
                {
                    finalState += $"\"{keyValuePair.Key}\": \"{keyValuePair.Value}\", ";
                }

            }
            finalState = finalState[0..^2] + "}"; //Remove last comma, finishes the JSON.

            MyLogger.DoLog($"Updating data for {device.Name}...", 4);

            await MQTT.SendMQTTMessageAsync(device.State_topic, finalState, device.Retain);
        }

        public static bool ReloadAutoConfig(int SingleDP = 0)
        {
            ClearAutoConfig(SingleDP);
            Thread.Sleep(1000);
            SendAutoConfig(SingleDP);
            return true;
        }

        public static bool ClearAutoConfig(int SingleDP = 0)
        {
            foreach (Device device in deviceList)
            {
                if (SingleDP == 0 || device.DP == SingleDP)
                {
                    MyLogger.DoLog("Removing Auto Config data for " + device.Name + "...", false);
                    //if (Program.Settings.GENERAL_DEBUGMODE) { MyLogger.DoLog($"{Program.Settings.HOMEASSISTANT_DISCOVERYTOPIC}/{device.Config_topic}/config", true); }
                    MQTT.SendMQTTMessageAsync(device.Config_topic + "", "", false).Wait(new TimeSpan(0, 0, 0, 0, 50));
                    foreach (KeyValuePair<string, string> conf in device.AutoConfigs)
                    {
                        //if (Program.Settings.GENERAL_DEBUGMODE) { MyLogger.DoLog($"{Program.Settings.HOMEASSISTANT_DISCOVERYTOPIC}/{conf.Key}/config", true); }
                        MQTT.SendMQTTMessageAsync($"{conf.Key}", "", false).Wait(new TimeSpan(0, 0, 0, 0, 50));
                    }
                    MyLogger.DoLog("OK", 3, true, 10);
                }
            }
            return true;
        }
        public static bool SendAutoConfig(int SingleDP = 0)
        {

            foreach (Device device in deviceList)
            {
                if (SingleDP == 0 || device.DP == SingleDP)
                {
                    MyLogger.DoLog("Sending Auto Config data for " + device.Name + "...", false);
                    foreach (KeyValuePair<string, string> conf in device.AutoConfigs)
                    {
                        if (Program.Settings.GENERAL_DEBUGMODE) { MyLogger.DoLog($"{conf.Key}", true); }
                        MQTT.SendMQTTMessageAsync($"{conf.Key}", conf.Value, false).Wait(new TimeSpan(0, 0, 0, 0, 250));
                        Thread.Sleep(300);
                    }
                    MyLogger.DoLog("OK", 3, true, 10);
                }
            }
            return true;
        }

    }
}