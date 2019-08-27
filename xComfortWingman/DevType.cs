using System;
using System.Collections.Generic;
using System.Text;

namespace xComfortWingman
{
    public static class DevType //: Basic.Device
    {
        // For declaring new types "on the fly"
        //public DevType(global::System.String name, List<Value> dataValues)
        //{
        //    Name = name;
        //    DataValues = dataValues;
        //}

        public string Name { get; }
        public List<Value> DataValues { get; }
    }
        public static DevType SwitchingActuator()
        {
            Name = "Switching actuator";
            DataValues = new List<Value> { SwitchedOn, RSSI, Battery };
        }
        public static DevType DimmingActuator()
        {
            Name = "Dimming actuator";
            DataValues = new List<Value> { DimLevel, RSSI, Battery };
        }
        public static DevType JalousieActuator()
        {
            Name = "Jalousie actuator";
            DataValues = new List<Value> { DimLevel, RSSI, Battery };
        }

        public static DevType BinaryInput()
        {
            Name = "Binary input";
            DataValues = new List<Value> { BinaryValue, RSSI, Battery };
        }

        public static DevType RoomController()
        {
            Name = "Room controller";
            DataValues = new List<Value> { Temperature, WheelPosition, RSSI, Battery };
        }

        public static DevType RoomController2()
        {
            Name = "Room controller with humidity";
            DataValues = new List<Value> { Temperature, WheelPosition, Humidity, RSSI, Battery };
        }

        public static DevType TemperatureSensor()
        {
            Name = "Temperature sensor";
            DataValues = new List<Value> { Temperature, RSSI, Battery };
        }
    }



