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
    }
}
