using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace xComfortWingman
{
    #region "Enums"

    public enum MQTT_CONNECTION_MODE
    {
        TCP,
        WEBSOCKET
    }

    public enum CI_CONNECTION_MODE
    {
        USB_MODE,
        RS232_MODE
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
        public bool RAW_ENABLED { get; set; } = true;
        public int RMF_TIMEOUT { get; set; } = 5000;
        public bool DEBUGMODE { get; set; } = true;
        public String DATAPOINTS_FILENAME { get; set; } = "datenpunkte.txt";


        // MQTT related settings
        public MQTT_CONNECTION_MODE MQTT_CONNECTION_MODE { get; set; } = MQTT_CONNECTION_MODE.TCP;
        public string MQTT_SERVER_WEBSOCKET { get; set; } = "destiny.geving.it:1883/mqtt";
        public string MQTT_SERVER_TCP { get; set; } = "destiny.geving.it";
        public string MQTT_CLIENT_ID { get; set; } = "bachelorpad_%rnd%";
        public string MQTT_CRED_USERNAME { get; set; } = "";
        public string MQTT_CRED_PASSWORD { get; set; } = "";
        public bool MQTT_USE_TLS { get; set; } = false;
        public bool MQTT_CLEAN_SESSION { get; set; } = false;
        public string MQTT_BASEPATH { get; set; } = "BachelorPad/xComfort";
        public string[] MQTT_EXTRA_SUBS { get; set; } = { "example/path", "many/may/go/here", "all/will/#/be/subscribed/to" };


        // Communication Interface related settings
        public CI_CONNECTION_MODE CONNECTION_MODE { get; set; } = CI_CONNECTION_MODE.USB_MODE;
        public byte[][] INTERFACE_INIT_COMMANDS { get; set; } = new byte[10][]; //Allow upto ten sets of commands to be executed at startup


        // RS232 related settings (used if CI is connecting in RS232 mode)
        public string RS232_PORT { get; set; } = "COM1";
        public int RS232_BAUD { get; set; } = 57600;
        public bool RS232_FLOW { get; set; } = false;
        public bool RS232_CRC { get; set; } = false;
        public byte RS232_STARTBYTE { get; set; } = 0x5A;
        public byte RS232_STOPTBYTE { get; set; } = 0xA5;

        public Settings DefaultSettings()
        {
            return new Settings();
        }

        public Settings(bool loadFromFile)
        {
            if (VerifySettingsFile())
            {
                LoadSettings();
            }
            else
            {
                ResetToDefault();
            }
        }

        public Settings()
        {
         //Keeping this blank will return the defaults until anything else is changed.
        }

        public bool ResetToDefault()
        {
            return WriteSettingsToFile(DefaultSettings());
        }

        public void LoadSettings()
        {
            Settings settings = ReadSettingsFromFile();
            RAW_ENABLED = settings.RAW_ENABLED;
            RMF_TIMEOUT = settings.RMF_TIMEOUT;
            DEBUGMODE = settings.DEBUGMODE;
            MQTT_CONNECTION_MODE = settings.MQTT_CONNECTION_MODE;
            MQTT_SERVER_WEBSOCKET = settings.MQTT_SERVER_WEBSOCKET;
            MQTT_SERVER_TCP = settings.MQTT_SERVER_TCP;
            MQTT_CLIENT_ID = settings.MQTT_CLIENT_ID;
            MQTT_CRED_USERNAME = settings.MQTT_CRED_USERNAME;
            MQTT_CRED_PASSWORD = settings.MQTT_CRED_PASSWORD;
            MQTT_USE_TLS = settings.MQTT_USE_TLS;
            MQTT_CLEAN_SESSION = settings.MQTT_CLEAN_SESSION;
            MQTT_BASEPATH = settings.MQTT_BASEPATH;
            MQTT_EXTRA_SUBS = settings.MQTT_EXTRA_SUBS;
            CONNECTION_MODE = settings.CONNECTION_MODE;
            INTERFACE_INIT_COMMANDS = settings.INTERFACE_INIT_COMMANDS;
            RS232_PORT = settings.RS232_PORT;
            RS232_BAUD = settings.RS232_BAUD;
            RS232_FLOW = settings.RS232_FLOW;
            RS232_CRC = settings.RS232_CRC;
            RS232_STARTBYTE = settings.RS232_STARTBYTE;
            RS232_STOPTBYTE = settings.RS232_STOPTBYTE;
        }

        public bool WriteSettingsToFile(Settings settings)
        {
            try
            {
                using (StreamWriter w = new StreamWriter("settings.json"))
                {
                    string json = JsonConvert.SerializeObject(settings);
                    w.WriteLine(json);
                }
                return true;
            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message,5);
                return false;
            }
        }

        public bool WriteSettingsToFile(string json)
        {
            try
            {
                using (StreamWriter w = new StreamWriter("settings.json"))
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
            return JsonConvert.SerializeObject(ReadSettingsFromFile());
        }

        private Settings ReadSettingsFromFile()
        {
            try
            {
                Settings settings;
                using (StreamReader r = new StreamReader("settings.json"))
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

        private bool VerifySettingsFile()
        {
            try
            {
                Settings settings;
                using (StreamReader r = new StreamReader("settings.json"))
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
