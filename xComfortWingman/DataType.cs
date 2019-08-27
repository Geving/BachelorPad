using System;
using System.Collections.Generic;
using System.Text;

namespace xComfortWingman
{
    public class DataType
    {
        
        string Name;
        string Unit;

        public class RSSI 
        {
            readonly string Name = "RSSI";
            int Value { get; set; }
            readonly string Unit = "dB";
        }

        public class Battery
        {
            new string Name = "Battery";
            int Value { get; set; }
            new string Unit = "";
        }

        public class DimmingLevel : DataType
        {
            new string Name = "DimmingLevel";
            int Value { get; set; }
            new string Unit = "%";
        }

        public class SwitchingLevel : DataType
        {
            new string Name = "SwitchingLevel";
            bool Value { get; set; }
            new string Unit = "";
        }

        public class Temperature : DataType
        {
            new string Name = "Temperature";
            float Value { get; set; }
            new string Unit = "°C";
        }

        public class Humidity : DataType
        {
            new string Name = "Humidity";
            float Value { get; set; }
            new string Unit = "%";
        }

        public class WheelPosition : DataType
        {
            new string Name = "WheelPosition";
            float Value { get; set; }
            new string Unit = "";
        }
    }
}
