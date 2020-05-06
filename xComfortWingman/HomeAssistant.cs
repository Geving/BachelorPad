using System;
using System.Collections.Generic;
using System.Text;

namespace xComfortWingman
{
    public class HomeAssistant
    {
        readonly static string BaseTopic = "myhome/homeassistant";
        public static List<Device> deviceList = new List<Device>();

        public static Device SetupNewDevice(Datapoint datapoint, string name = "")
        {
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
            if (name != "") newDevice.Name = name;
            deviceList.Add(newDevice);
            return newDevice;
        }

        public partial class Device
        {
            public int DP { get; set; }
            public string devtopic { get; set; }
            public string currentState { get; set; }

            public string Name { get; set; }
            public string Manufacturer { get; set; } = "xComfort";
            public string Model { get; set; }

            //For the device
            //public string Device { get; set; }
            public string Connections { get; set; }
            public string Identifiers { get; set; }
            public string Sw_version { get; set; }
            public string Via_device { get; set; } = "Wingman";

            //Common for all types
            public string Availability_topic { get; set; } = "availability";
            public string Payload_available { get; set; } = "online";
            public string Payload_not_available { get; set; } = "offline";
            public string Payload_off { get; set; } = "ON";
            public string Payload_on { get; set; } = "OFF";
            public int Qos { get; set; } = 0;
            public bool Retain { get; set; } = false;
            public string State_on { get; set; } = "ON";
            public string State_off { get; set; } = "OFF";
            public string State_topic { get; set; } = "state";
            public string Value_template { get; set; } 

            public string GetConfig()
            {
                return "";
            }
        }

        class Switch : Device
        {
            public readonly string devtype = "Switch";
            public string Command_topic { get; set; }
            public string Icon { get; set; }
            public string Json_attributes_template { get; set; }
            public string Json_attributes_topic { get; set; }
            public bool Optimistic { get; set; }

            public Switch(Datapoint datapoint)
            {
                this.devtopic = $"{BaseTopic}/{devtype}/{datapoint.Name}";
                //Return based solely on the datapoint provided
                this.Name = datapoint.Name;
                this.Model = GetModelNameForDeviceType(datapoint.Type);
                this.Availability_topic = $"{devtopic}/{Availability_topic}";
                this.DP = datapoint.DP;
                this.Identifiers = datapoint.DP.ToString() + datapoint.Serial.ToString();
                this.Command_topic = $"{devtopic}/set";

            }

            public new string GetConfig()
            {
                return $"{{ \"name\": \"{this.Name}\", \"command_topic\": \"{this.Command_topic}\", \"state_topic\": \"{this.State_topic}\"}}";
            }

           
        }

        partial class SensorBase : Device
        {
            public int Expire_after { get; set; }
            public bool Force_update { get; set; }

        }

        class BinarySensor : SensorBase
        {
            private static string[] BinarySensorClass = { "None", "battery", "battery_charging", "cold", "connectivity", "door", "garage_door", "gas", "heat", "light", "lock", "moisture", "motion", "moving", "occupancy", "opening", "plug", "power", "presence", "problem", "safety", "smoke", "sound", "vibration", "window" };
            public readonly string devtype = "BinarySensor";
            public string Device_class { get; set; }// = BinarySensorClass[0];
            public int off_delay { get; set; }
            public string unique_id { get; set; }

            public BinarySensor(Datapoint datapoint)
            {
                this.devtopic = $"{BaseTopic}/{devtype}/{datapoint.Name}";
                //Return based solely on the datapoint provided
                this.Name = datapoint.Name;
                this.Model = GetModelNameForDeviceType(datapoint.Type);
                this.Availability_topic = $"{devtopic}/{Availability_topic}";
                this.DP = datapoint.DP;
                this.Identifiers = datapoint.DP.ToString() + datapoint.Serial.ToString();
                this.off_delay = 0;
                this.Device_class = GetSensorTypeForDeviceType(datapoint.Type);
                this.Expire_after = GetExpiryForDeviceType(datapoint.Type);
            }
        }

        class Sensor : SensorBase
        {
            //private static string[] SensorClass = { "None", "battery", "humidity", "illuminance", "signal_strength", "temperature", "power", "pressure", "timestamp" };
            public readonly string devtype = "Sensor";
            public string Device_class { get; set; } = "None";
            public string Unit_of_measurement { get; set; } = "";
            public string Icon { get; set; } = "";

            public Sensor(Datapoint datapoint)
            {
                this.devtopic = $"{BaseTopic}/{devtype}/{datapoint.Name}";
                //Return based solely on the datapoint provided
                this.Name = datapoint.Name;
                this.Model = GetModelNameForDeviceType(datapoint.Type);
                this.Availability_topic = $"{devtopic}/{Availability_topic}";
                this.DP = datapoint.DP;
                this.Identifiers = datapoint.DP.ToString() + datapoint.Serial.ToString();
                this.Device_class = GetSensorTypeForDeviceType(datapoint.Type);
                this.Expire_after = GetExpiryForDeviceType(datapoint.Type);
                this.Unit_of_measurement = GetSensorDataUnitForDeviceType(datapoint.Type);
            }
        }

        class Light : Device
        {
            public readonly string devtype = "Light";
            public string brightness_command_topic { get; set; }
            public int brightness_scale { get; set; }
            public string brightness_state_topic { get; set; }
            public string brightness_value_template { get; set; }
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
                this.devtopic = $"{BaseTopic}/{devtype}/{datapoint.Name}";
                //Return based solely on the datapoint provided
                this.Name = datapoint.Name;
                this.Model = GetModelNameForDeviceType(datapoint.Type);
                this.Availability_topic = $"{devtopic}/{Availability_topic}";
                this.DP = datapoint.DP;
                this.Identifiers = datapoint.DP.ToString() + datapoint.Serial.ToString();
                this.brightness_scale = 100;
                this.brightness_command_topic = $"{devtopic}/set";
                this.brightness_state_topic = $"{devtopic}/state";

            }

            public new string GetConfig()
            {
                return $"{{" + "\r\n" +
                          $"\"name\": \"{this.Name}\"," + "\r\n" +
                          $"\"unique_id\": \"{this.Identifiers}\"," + "\r\n" +
                          $"\"cmd_t\": \"{this.brightness_command_topic}\"," + "\r\n" +
                          $"\"stat_t\": \"{this.brightness_state_topic}\"," + "\r\n" +
                          $"\"schema\": \"json\"," + "\r\n" +
                          $"\"brightness\": true" + "\r\n" +
                        $"}}";
            }
                       
        }

        class Trigger : Device
        {
            private static string[] types = { "button_short_press", "button_short_release", "button_long_press", "button_long_release", "button_double_press", "button_triple_press", "button_quadruple_press", "button_quintuple_press" };
            private static string[] subtypes = { "turn_on", "turn_off", "button_1", "button_2", "button_3", "button_4", "button_5", "button_6" };

            public string devtype = "trigger";
            public string automation_type = "trigger";
            public string payload;
            public string topic;
            public string type = types[0];
            public string subtype = subtypes[0];

            public Trigger(string automation_type, string payload, string topic, string type, string subtype)
            {
                this.automation_type = automation_type;
                this.payload = payload;
                this.topic = topic;
                this.type = type;
                this.subtype = subtype;
            }
        }

        class Cover : Device
        {
            public string devtype = "Cover";
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

        }

        class Lock : Device
        {
            public string devtype = "Lock";
            public string payload_lock;
            public string payload_unlock;
            public string state_locked = "LOCKED";
            public string state_unlocked = "UNLOCKED";

        }

        class HVAC : Device
        {
            public string devtype = "HVAC";
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

            
        }

        static string GetModelNameForDeviceType(int type)
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
                case 7: //Dimming Actuator
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
                case 17: //Binary Input, Battery
                    return "Binary Input, Battery";
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
        
        static string GetSensorTypeForDeviceType(int type)
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

        static int GetExpiryForDeviceType(int type)
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
    }
}
