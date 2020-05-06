using System;
using System.Collections.Generic;
using System.Text;

namespace xComfortWingman
{
    class Simple
    {
        public static List<Device> devices = new List<Device>();

        public class Device // Represents a Datapoint. Datapoints represents xComfort devices.
        {
            public string Name { get; set; }                                                           //homie/DimKitchen/$name 			"Kitchen lights"
            public string Nodes { get; set; }                                                          //homie/DimKitchen/$nodes			"lights,signal"
            public string State { get; set; }                                                          //homie/DimKitchen/$state			"ready"

            public List<Homie.Node> Node { get; set; } = new List<Homie.Node>();

            // Not part of the Homie specs
            public Datapoint Datapoint { get; set; }
        }



        string DeviceJSON(Datapoint datapoint, Protocol.PT_RX.Packet packet)
        {
            string Name = datapoint.Name;
            string output = $"{{\n" +
                            $"/t\"name\":\"{Name}\",\n" +
                            $"/t\"id\":\"{datapoint.DP}\",\n" +
                            $"/t\"value\":[\n" +
                            $"/t/t\"RSSI\":\"{packet.MGW_RX_RSSI.ToString()}\"," +
                            $"/t/t\"Battery\":{packet.MGW_RX_BATTERY.ToString()}\",";

            //$"/t/t\"{}\":\"{}\",\n";

            //    default: { node.Value = CI.GetDataFromPacket(packet.MGW_RX_DATA, packet.MGW_RX_DATA_TYPE, ""); break; }




            switch (datapoint.Type)
            {
                case 6: //Switching Actuator
                    output += $"/t/t\"Dimmer\":\"{packet.MGW_RX_INFO_SHORT.ToString()}\",";
                    break;
                case 7: //Dimming Actuator
                    output += $"/t/t\"Dimmer\":\"{packet.MGW_RX_INFO_SHORT.ToString()}\",";
                    break;

                case 8: //Jalousie Actuator
                case 29: //Jalousie Actuator with Security
                    output += $"/t/t\"Dimmer\":\"{packet.MGW_RX_INFO_SHORT.ToString()}\",";
                    break;

                case 19: //Home-Manager
                    break;
                case 27: //Analog Actuator
                    break;
                case 28: //Room-Manager
                    break;
                case 30: //Communication Interface
                    break;

                case 35: //Room Controller w/ Switch/Humidity
                case 36: //Room Controller w/ Switch/Humidity
                case 37: //Room Controller w/ Switch/Humidity
                case 38: //Room Controller w/ Switch/Humidity
                    output += $"/t/t\"Controller\":\"{CI.GetDataFromPacket(packet.MGW_RX_DATA, packet.MGW_RX_DATA_TYPE, "")}\",";
                    output += $"/t/t\"Dimmer\":\"{packet.MGW_RX_INFO_SHORT.ToString()}\",";
                    break;

                case 60: //Multi Channel Heating Actuator
                case 61: //Multi Channel Heating Actuator
                case 62: //Multi Channel Heating Actuator
                    break;

                case 63: //Communication Interface USB
                    break;

                case 64: //Switching Actuator New Generation
                case 65: //Switching Actuator New Generation
                case 66: //Switching Actuator New Generation
                case 67: //Switching Actuator New Generation
                case 68: //Switching Actuator New Generation
                case 69: //Switching Actuator New Generation
                    break;

                default: // Device is unknown, not implemented or not accepting input.
                    break;
            }
            return output;
        }
    }
}
