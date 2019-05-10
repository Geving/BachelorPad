using System;
using System.Collections.Generic;
using System.Text;
using xComfortWingman.Protocol;

namespace xComfortWingman
{
    public class DeviceType
    {
        public int ID { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
        public int[] Channels { get; set; }
        public int[] Modes { get; set; }
        public byte[] MessageTypes { get; set; }
        public byte[] DataTypes { get; set; }
        public string Comment { get; set; }
        public byte RSSI { get; set; }
        public byte Battery { get; set; }

        public DeviceType(int iD, string name, string shortName, int number, int[] channels, int[] modes, byte[] messageTypes, byte[] dataTypes, string comment)
        {
            ID = iD;
            Name = name;
            ShortName = shortName;
            Number = number;
            Channels = channels;
            Modes = modes;
            MessageTypes = messageTypes;
            DataTypes = dataTypes;
            Comment = comment;
        }

        public string[] GetProperties()
        {
            List<string> tmp = new List<string>();
            foreach (byte msgType in MessageTypes)
            {
                tmp.Add(PT_RX.MGW_RX_MSG_TYPE.GetNameFromByte(msgType));
            }
            return tmp.ToArray();
        }

        public string GetSafeName()
        {
            Dictionary<int, string> dict = new Dictionary<int, string>
            {
                { 1, "Push-button-Single" },
                { 2, "Push-button-Dual" },
                { 4, "Push-button Quad" },
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

            dict.TryGetValue(System.Convert.ToInt32(Number), out string tmp);
            return tmp;
        }
    }
}
