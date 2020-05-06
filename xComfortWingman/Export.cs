using System;
using System.Collections.Generic;
using System.Text;

namespace xComfortWingman
{
    static class Export
    {
        private static readonly string devicePrefix = "Simple";
        private static readonly string broker = "06baebd6";

        private static readonly Dictionary<string, string> keyValuePairs = new Dictionary<string, string>() {
            { "integer", "Number" },
            { "integer%", "Dimmer" },
            { "decimal","Number" },
            { "float","Number" },
            { "number","Number" },
            { "string","String" },
            { "text","String" },
            { "Dimmer","Dimmer" },
            { "Boolean","Switch" },
            { "Switch","Switch" },
            { "Contact","Contact" },
            { "DateTime","DateTime" },
            { "Rollershutter","Rollershutter" }
        };
        
        
        /*
           "mqtt_topic_SimpleThermostatLivingroom_ThermoLivingroomWheel -\u003e mqtt:topic:SimpleThermostatLivingroom:ThermoLivingroomWheel": {
    "class": "org.eclipse.smarthome.core.thing.link.ItemChannelLink",
    "value": {
      "channelUID": {
        "segments": [
          "mqtt",
          "topic",
          "SimpleThermostatLivingroom",
          "ThermoLivingroomWheel"
        ]
      },
      "configuration": {
        "properties": {}
      },
      "itemName": "mqtt_topic_SimpleThermostatLivingroom_ThermoLivingroomWheel"
    }
  },
  "mqtt_topic_SimpleThermostatLivingroom_LivingroomThermostatTemperature -\u003e mqtt:topic:SimpleThermostatLivingroom:LivingroomThermostatTemperature": {
    "class": "org.eclipse.smarthome.core.thing.link.ItemChannelLink",
    "value": {
      "channelUID": {
        "segments": [
          "mqtt",
          "topic",
          "SimpleThermostatLivingroom",
          "LivingroomThermostatTemperature"
        ]
      },
      "configuration": {
        "properties": {}
      },
      "itemName": "mqtt_topic_SimpleThermostatLivingroom_LivingroomThermostatTemperature"
    }
  }, 
         */
        public static string GetItemChannelLinkJSONfromDevice(Homie.Device device)
        {
            //keyValuePairs.  .Add(new KeyValuePair<string, string>("abc", "def"));
            string ItemChannelLinkJSON = "";
            for (int i = 0; i < device.Node.Count - 1; i++)
            {
                ItemChannelLinkJSON += "{\r\n";
                //"mqtt_topic_SimpleThermostatLivingroom_ThermoLivingroomWheel -\u003e mqtt:topic:SimpleThermostatLivingroom:ThermoLivingroomWheel": {
                ItemChannelLinkJSON += $"\"mqtt_topic_{devicePrefix}{device.Name}_{device.Node[i].Name} -\u003e mqtt:topic:{devicePrefix}{device.Name}:{device.Node[i].Name}\": {{\r\n";
                //  "class": "org.eclipse.smarthome.core.thing.link.ItemChannelLink",
                ItemChannelLinkJSON += $"\"class\": \"org.eclipse.smarthome.core.thing.link.ItemChannelLink\",\r\n";
                //  "value": {
                ItemChannelLinkJSON += $"\"value\": {{\r\n";
                //    "channelUID": {
                ItemChannelLinkJSON += $"    \"channelUID\": {{\r\n";
                //      "segments": [
                ItemChannelLinkJSON += $"      \"segments\": [\r\n";
                //        "mqtt",
                ItemChannelLinkJSON += $"        \"mqtt\",\r\n";
                //        "topic",
                ItemChannelLinkJSON += $"        \"topic\",\r\n";
                //        "SimpleThermostatLivingroom",
                ItemChannelLinkJSON += $"        \"{devicePrefix}{device.Name}\",\r\n";
                //        "ThermoLivingroomWheel"
                ItemChannelLinkJSON += $"        \"{device.Node[i].Name}\"\r\n";
                //      ]
                ItemChannelLinkJSON += $"      ]\r\n";
                //    },
                ItemChannelLinkJSON += $"    }},\r\n";
                //    "configuration": {
                ItemChannelLinkJSON += $"    \"configuration\": {{\r\n";
                //      "properties": {}
                ItemChannelLinkJSON += $"      \"properties\": {{}}\r\n";
                //    },
                ItemChannelLinkJSON += $"    }},\r\n";
                //    "itemName": "mqtt_topic_SimpleThermostatLivingroom_ThermoLivingroomWheel"
                ItemChannelLinkJSON += $"    \"itemName\": \"mqtt_topic_{devicePrefix}{device.Name}_{device.Node[i].Name}\"\r\n";
                //  }
                ItemChannelLinkJSON += $"  }}\r\n";
                //},
                ItemChannelLinkJSON += $"}},\r\n";
            }
            if (ItemChannelLinkJSON.Length > 0)
            {
                ItemChannelLinkJSON = ItemChannelLinkJSON.Remove(ItemChannelLinkJSON.Length - 3, 1); //Remove the last comma
            }
            return ItemChannelLinkJSON;
        }

        public static string GetCoreThingJSONfromDevice(Homie.Device device)
        {
            string CoreThingJSON = $"{{\r\n";

            CoreThingJSON += $"\"mqtt:topic:{devicePrefix}{device.Name}\": {{\r\n";
            CoreThingJSON += $"  \"class\": \"org.eclipse.smarthome.core.thing.internal.ThingImpl\",\r\n";
            CoreThingJSON += $"  \"value\": {{\r\n";
            CoreThingJSON += $"    \"label\": \"NAMEHERE\",\r\n";
            CoreThingJSON += $"    \"bridgeUID\": {{\r\n";
            CoreThingJSON += $"      \"segments\": [\r\n";
            CoreThingJSON += $"        \"mqtt\",\r\n";
            CoreThingJSON += $"        \"broker\",\r\n";
            CoreThingJSON += $"        \"{broker}\"\r\n";
            CoreThingJSON += $"      ]\r\n";
            CoreThingJSON += $"    }},\r\n";
            CoreThingJSON += $"    \"channels\": [\r\n";
            foreach (Homie.Node node in device.Node)
            {
                foreach (Homie.Property property in node.PropertyList)
                {
                    CoreThingJSON += $"      {{\r\n";
                    CoreThingJSON += $"        \"acceptedItemType\": \"{keyValuePairs.GetValueOrDefault(property.DataType)}\",\r\n";
                    CoreThingJSON += $"        \"kind\": \"STATE\",\r\n";
                    CoreThingJSON += $"        \"uid\": {{\r\n";
                    CoreThingJSON += $"          \"segments\": [\r\n";
                    CoreThingJSON += $"            \"mqtt\",\r\n";
                    CoreThingJSON += $"            \"topic\",\r\n";
                    CoreThingJSON += $"            \"{node.Name}\",\r\n";
                    CoreThingJSON += $"            \"{node.Name}{property.Name}\"\r\n";
                    CoreThingJSON += $"          ]\r\n";
                    CoreThingJSON += $"        }},\r\n";
                    CoreThingJSON += $"        \"channelTypeUID\": {{\r\n";
                    CoreThingJSON += $"          \"segments\": [\r\n";
                    CoreThingJSON += $"            \"mqtt\",\r\n";
                    CoreThingJSON += $"            \"{property.DataType}\"\r\n";
                    CoreThingJSON += $"          ]\r\n";
                    CoreThingJSON += $"        }},\r\n";
                    CoreThingJSON += $"        \"label\": \"{property.Name}\",\r\n";
                    CoreThingJSON += $"        \"configuration\": {{\r\n";
                    CoreThingJSON += $"          \"properties\": {{\r\n";
                    CoreThingJSON += $"            \"stateTopic\": \"{Program.Settings.MQTT_BASETOPIC}/{node.Name}\",\r\n";
                    CoreThingJSON += $"            \"transformationPattern\": \"JSONPATH:$.{property.Name}\"\r\n";
                    CoreThingJSON += $"          }}\r\n";
                    CoreThingJSON += $"        }},\r\n";
                    CoreThingJSON += $"        \"properties\": {{}},\r\n";
                    CoreThingJSON += $"        \"defaultTags\": []\r\n";
                    CoreThingJSON += $"      }},\r\n";
                }
            }
            if (CoreThingJSON.Length > 0)
            {
                CoreThingJSON = CoreThingJSON.Remove(CoreThingJSON.Length - 3, 1); //Remove the last comma
            }
            
            CoreThingJSON += $"    ],\r\n";
            CoreThingJSON += $"    \"configuration\": {{\r\n";
            CoreThingJSON += $"      \"properties\": {{}}\r\n";
            CoreThingJSON += $"    }},\r\n";
            CoreThingJSON += $"    \"properties\": {{}},\r\n";
            CoreThingJSON += $"    \"uid\": {{\r\n";
            CoreThingJSON += $"      \"segments\": [\r\n";
            CoreThingJSON += $"        \"mqtt\",\r\n";
            CoreThingJSON += $"        \"topic\",\r\n";
            CoreThingJSON += $"        \"{device.Name}\"\r\n";
            CoreThingJSON += $"      ]\r\n";
            CoreThingJSON += $"    }},\r\n";
            CoreThingJSON += $"    \"thingTypeUID\": {{\r\n";
            CoreThingJSON += $"      \"segments\": [\r\n";
            CoreThingJSON += $"        \"mqtt\",\r\n";
            CoreThingJSON += $"        \"topic\"\r\n";
            CoreThingJSON += $"      ]\r\n";
            CoreThingJSON += $"    }},\r\n";
            CoreThingJSON += $"    \"location\": \"LOCATIONHERE\"\r\n";
            CoreThingJSON += $"  }}\r\n";
            CoreThingJSON += $"}}\r\n";

            return CoreThingJSON;
        }
    }
}
