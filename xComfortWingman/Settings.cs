using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace xComfortWingman
{
    #region "Enums"

    public enum MQTT_CONNECTION_METHOD
    {
        TCP,
        WEBSOCKET
    }

    public enum CI_CONNECTION_MODE
    {
        USB_MODE,
        RS232_MODE,
        SPECIAL_MODE,
        DUMMY_MODE
    }

    public enum CI_RS232_BAUD
    {
        BD_1200,
        BD_2400,
        BD_9600,
        BD_14400,
        BD_19200,
        BD_37500,
        BD_57600
    }
    #endregion
    public class Settings
    {
        
        // General settings
        public bool GENERAL_RAW_ENABLED { get; set; } = true;
        public int GENERAL_RMF_TIMEOUT { get; set; } = 5000;
        public bool GENERAL_DEBUGMODE { get; set; } = false;
        public String GENERAL_DATAPOINTS_FILENAME { get; set; } = "/mydata/datenpunkte.txt";
        public String GENERAL_DP_NAMES_FILENAME { get; set; } = "/mydata/names.txt";
        public String GENERAL_NAME { get; set; } = "xComfort";
        public bool GENERAL_FROM_FILE { get; set; } = false;
        public int GENERAL_FORECOLOR { get; set; } = (int)ConsoleColor.Gray;
        public int GENERAL_BACKCOLOR { get; set; } = (int)ConsoleColor.Black;
        public int GENERAL_WINDOWWIDTH { get; set; } = 120;
        public Boolean GENERAL_FILTER_DUPLICATE_DP { get; set; } = false;

        // MQTT related settings
        public MQTT_CONNECTION_METHOD MQTT_CONNECTION_METHOD { get; set; } = MQTT_CONNECTION_METHOD.TCP;
        public string MQTT_SERVER_WEBSOCKET { get; set; } = "mqtt.geving.it:1883/mqtt";
        public string MQTT_SERVER_TCP { get; set; } = "mqtt.geving.it";
        public string MQTT_CLIENT_ID { get; set; } = "wingman_%rnd%";
        public string MQTT_CRED_USERNAME { get; set; } = "";
        public string MQTT_CRED_PASSWORD { get; set; } = "";
        public bool MQTT_USE_TLS { get; set; } = false;
        public bool MQTT_CLEAN_SESSION { get; set; } = false;
        public string MQTT_BASETOPIC { get; set; } = "myhome";
        //public string[] MQTT_EXTRA_SUBS { get; set; } = { "example/path", "many/may/go/here", "all/will/+/be/subscribed/to" };

        // MQTT HOMIE related
        public bool HOMIE_USE_HOMIE { get; set; } = false;
        public string HOMIE_HOMIE { get; set; } = "3.0"; //{ $"{BaseTopic}$homie", "3.0" },
        public string HOMIE_NAME { get; set; } = "xComfort";    //{ $"{BaseTopic}$name", "3.0" },
        //public string HOMIE_LOCALIP { get; set; }                //{ $"{BaseTopic}$localip", "3.0" },
        //public string HOMIE_MAC { get; set; }                    //{ $"{BaseTopic}$mac", "3.0" },
        public string HOMIE_FW_NAME { get; set; } = "FW";          //{ $"{BaseTopic}$fw/name", "3.0" },
        public string HOMIE_FW_VERSION { get; set; } = "1.0";      //{ $"{BaseTopic}$fw/version", "3.0" },
        //public string HOMIE_NODES { get; set; }                  //{ $"{BaseTopic}$nodes", "3.0" },
        public string HOMIE_IMPLEMENTATION { get; set; } = "RPi3"; //{ $"{BaseTopic}$implementation", "raspberry" },
        public string HOMIE_STATS_INTERVAL { get; set; } = "60";   //{ $"{BaseTopic}$stats/interval", "60" },
        //public string HOMIE_STATE { get; set; }                  //{ $"{BaseTopic}$state", "ready" }

        //Simple JSON related settings
        public bool BASIC_USE_BASIC { get; set; } = false;

        public bool HOMEASSISTANT_USE_HOMEASSISTANT { get; set; } = true;

        public string HOMEASSISTANT_BASETOPIC { get; set; } = "myhome";
        public string HOMEASSISTANT_DISCOVERYTOPIC { get; set; } = "homeassistant";
        public string HOMEASSISTANT_AVAILABILITYTOPIC { get; set; } = "myhome/xcomfort2mqtt/availability";
        public bool HOMEASSISTANT_USE255ASMAX { get; set; } = false;
        public int HOMEASSISTANT_HEARTBEATINTERVAL { get; set; } = 60;
        public int HOMEASSISTANT_AUTOCONFIGINTERVAL { get; set; } = 60;

        // Communication Interface related settings
        public CI_CONNECTION_MODE CI_CONNECTION_MODE { get; set; } = CI_CONNECTION_MODE.USB_MODE;
        public byte[][] CI_INTERFACE_INIT_COMMANDS { get; set; } = new byte[10][]; //Allow upto ten sets of commands to be executed at startup
        public string CI_NAME { get; set; } = "/dev/ttyUSB0";

        // RS232 related settings (used if CI is connecting in RS232 mode)
        public string RS232_PORT { get; set; } = "COM1";
        public int RS232_BAUD { get; set; } = 57600;
        public bool RS232_FLOW { get; set; } = false;
        public bool RS232_CRC { get; set; } = false;
        public byte RS232_STARTBYTE { get; set; } = 0x5A;
        public byte RS232_STOPTBYTE { get; set; } = 0xA5;

        public static string SettingsFilePath(string DefPath = "")
        {
            string[] PossiblePaths = {
                "settings.json",
                "./mydata/settings.json",
                "/mydata/settings.json"
            };
            foreach(String path in PossiblePaths)
            if (System.IO.File.Exists(path)) { return path; }
            return DefPath;
        }

        public static Settings DefaultSettings()
        {
            return new Settings();
        }

        public static Settings GetSettings(bool loadFromFile = false)
        {
            Settings settings = new Settings();
            if (loadFromFile && VerifySettingsFile(SettingsFilePath()))
            {
                settings=ReadSettingsFromFile(SettingsFilePath());
                settings.GENERAL_FROM_FILE = true;
                WriteSettingsToFile(settings, SettingsFilePath("settings.json"));
            }
            else
            {
                ResetToDefault();
            }
            return settings;
        }
        

        //public Settings()
        //{
        // //Keeping this blank will return the defaults until anything else is changed.
        //}

        public static bool ResetToDefault()
        {
            return WriteSettingsToFile(DefaultSettings(), SettingsFilePath("settings.json"));
        }

        public static bool WriteSettingsToFile(Settings settings, string FilePath)
        {
            try
            {
                using (StreamWriter w = new StreamWriter(FilePath))
                {
                    string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                    w.WriteLine(json);
                }
                return true;
            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message,5);
                return false;
            }
        }

        public bool WriteSettingsToFile(string json, string FilePath)
        {
            try
            {
                using (StreamWriter w = new StreamWriter(FilePath))
                {
                    w.WriteLine(json);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message,5);
                return false;
            }
        }

        public string GetSettingsAsJSON()
        {
            return JsonConvert.SerializeObject(ReadSettingsFromFile(SettingsFilePath()));
        }

        public static Settings ReadSettingsFromFile(string FilePath)
        {
            if(FilePath == "") { return DefaultSettings(); }
            try
            {
                Settings settings;
                using (StreamReader r = new StreamReader(FilePath))
                {
                    string json = r.ReadToEnd();
                    settings = JsonConvert.DeserializeObject<Settings>(json);
                }
                return settings;
            }
            catch
            {
                return DefaultSettings();
            }
        }

        private static bool VerifySettingsFile(string FilePath)
        {
            if (FilePath == "") { return false; }
            try
            {
                Settings settings;
                using (StreamReader r = new StreamReader(FilePath))
                {
                    string json = r.ReadToEnd();
                    settings = JsonConvert.DeserializeObject<Settings>(json);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
