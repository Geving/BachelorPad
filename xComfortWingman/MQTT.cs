using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Net.NetworkInformation;
using static xComfortWingman.MyLogger;
using System.Net;

namespace xComfortWingman
{
    public class MQTT
    {
        

        private static IMqttClient mqttClient;

        public static async Task RunMQTTClientAsync()
        {
            try
            {
                Settings settings = Program.Settings;
                var factory = new MqttFactory();
                mqttClient = factory.CreateMqttClient();

                var ClientID = settings.MQTT_CLIENT_ID.Replace("%rnd%", new Guid().ToString());

                var Credentials = new MqttClientCredentials
                {
                    Username = settings.MQTT_CRED_USERNAME,
                    Password = settings.MQTT_CRED_PASSWORD
                };

                var TlsOptions = new MqttClientTlsOptions
                {
                    UseTls = settings.MQTT_USE_TLS
                };

                var ChannelOptions_WebSocket = new MqttClientWebSocketOptions
                {
                    Uri = settings.MQTT_SERVER_WEBSOCKET,
                    TlsOptions = TlsOptions
                };

                var ChannelOptions_TCP = new MqttClientTcpOptions
                {
                    Server = settings.MQTT_SERVER_TCP,
                    TlsOptions = TlsOptions
                };

                var clientOptions = new MqttClientOptions();

                if (settings.MQTT_CONNECTION_MODE == MQTT_CONNECTION_MODE.TCP)
                {
                    clientOptions = new MqttClientOptions
                    {
                        CleanSession = settings.MQTT_CLEAN_SESSION,
                        ClientId = ClientID,
                        Credentials = Credentials,
                        ChannelOptions = ChannelOptions_TCP,
                        CommunicationTimeout = TimeSpan.FromSeconds(3)
                    };
                }
                else
                {
                    clientOptions = new MqttClientOptions
                    {
                        CleanSession = settings.MQTT_CLEAN_SESSION,
                        ClientId = ClientID,
                        Credentials = Credentials,
                        ChannelOptions = ChannelOptions_WebSocket,
                        CommunicationTimeout = TimeSpan.FromSeconds(3)
                    };

                }


                mqttClient.ApplicationMessageReceived += async (s, e) =>
                {
                    string[] topics = e.ApplicationMessage.Topic.Split("/");
                    int basePathLevels = settings.MQTT_BASETOPIC.Split("/").Length;
                    if (topics.Length < basePathLevels + 1)
                    {
                        DoLog("Invalid topic path!");
                        return;
                    }
                    switch (topics[basePathLevels])
                    {
                        case "cmd":
                            {
                                try
                                {
                                    await CI.SendData(Convert.ToInt32(topics[basePathLevels + 1]), Convert.ToInt32(Encoding.UTF8.GetString(e.ApplicationMessage.Payload)));
                                }
                                catch (Exception exception)
                                {
                                    LogException(exception);
                                }

                                break;
                            }
                        case "get":
                        case "set":
                            {
                                switch (topics[basePathLevels + 1])
                                {
                                    case "config":
                                        {
                                            if (topics[basePathLevels] == "get")
                                            {
                                                await SendMQTTMessageAsync($"BachelorPad/xComfort/{topics[basePathLevels]}/{topics[basePathLevels + 1]}/result", Program.Settings.GetSettingsAsJSON());
                                            }
                                            else
                                            {
                                                await SendMQTTMessageAsync($"BachelorPad/xComfort/{topics[basePathLevels]}/{topics[basePathLevels + 1]}/result", Program.Settings.WriteSettingsToFile(Encoding.UTF8.GetString(e.ApplicationMessage.Payload)).ToString());
                                            }
                                            break;
                                        }
                                    case "datapoints":
                                        {
                                            if (topics[basePathLevels] == "get")
                                            {
                                                await SendMQTTMessageAsync($"BachelorPad/xComfort/{topics[basePathLevels]}/{topics[basePathLevels + 1]}/result", CI.GetDatapointFile());
                                            }
                                            else
                                            {
                                                await SendMQTTMessageAsync($"BachelorPad/xComfort/{topics[basePathLevels]}/{topics[basePathLevels + 1]}/result", CI.SetDatapointFile(Encoding.UTF8.GetString(e.ApplicationMessage.Payload)).ToString());
                                            }
                                            break;
                                        }
                                    case "datapoint":
                                        {
                                            if (topics[basePathLevels] == "get")
                                            {
                                                //This makes no sense.
                                            }
                                            else
                                            {
                                                await SendMQTTMessageAsync($"BachelorPad/xComfort/{topics[basePathLevels]}/{topics[basePathLevels + 1]}/result", CI.ImportDatapointsOneByOne(Encoding.UTF8.GetString(e.ApplicationMessage.Payload)).ToString());
                                            }
                                            break;
                                        }
                                    default:
                                        {
                                            break;
                                        }
                                }
                            break;
                        }   
                        case "RAW":
                            {
                                //Because we only subscribe to RAW/in, we don't need to check topics[3]. It MUST be "in" to trigger this code.

                                // The HEX data comes as plain text, which is right now stored as a byte array.
                                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload).Replace(" ", ""); //Get the text from the payload and remove the spaces
                                int NumberChars = payload.Length;
                                byte[] payloadAsBytes = new byte[(NumberChars / 2)];
                                for (int i = 0; i < NumberChars; i += 2) //Go through the payload two bytes pr step
                                {
                                    payloadAsBytes[i / 2] = Convert.ToByte(payload.Substring(i, 2), 16); //Convert "two bytes of text" into one byte of "data"
                                }
                                //if (payloadAsBytes[0] != 0x00) { payloadAsBytes = AddZeroAsFirstByte(payloadAsBytes); }
                                //if (Settings.CONNECTION_MODE==CI_CONNECTION_MODE.RS232_MODE) { payloadAsBytes = AddRS232Bytes(payloadAsBytes); }
                                CI.PrintByte(payloadAsBytes, "Sending RAW data");
                                await CI.SendData(payloadAsBytes); //Send this to the interface for immediate transmit
                                break;
                            }
                        default:
                            {
                                DoLog("Unknown topic: " + topics[2]);
                                break;
                            }
                    }
                };


                mqttClient.Connected += async (s, e) =>
                {
                    try
                    {
                        // Subscribe to a topic
                        await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("BachelorPad/xComfort/cmd/#").Build());
                        await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("BachelorPad/xComfort/RAW/in").Build());
                        await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("BachelorPad/xComfort/get/#").Build());
                        await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("BachelorPad/xComfort/set/#").Build());
                        //await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("BachelorPad/xComfort/RAW/out").Build()); // We don't need to subscribe to our own outbound messages...
                    } catch (Exception exception)
                    {
                        LogException(exception);
                    }
                };

                mqttClient.Disconnected += async (s, e) =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));

                    //DoLog("Reconnecting to MQTT server...", false);
                    try
                    {
                        await mqttClient.ConnectAsync(clientOptions);
                        if (mqttClient.IsConnected)
                        {
                            //DoLog("OK", 3, true, 10);
                        }
                        else
                        {
                            DoLog("Reconnecting to MQTT server...", false);
                            DoLog("FAIL", 3, true, 14);
                        }
                    } catch (Exception exception)
                    {
                        DoLog("Reconnecting to MQTT server...", false);
                        DoLog("ERROR", 3, true, 12);
                        LogException(exception);
                    }
                };

                try
                {
                    DoLog("Connecting to MQTT server...", false);
                    await mqttClient.ConnectAsync(clientOptions);
                    if (mqttClient.IsConnected)
                    {
                        DoLog("OK", 3, false, 10);
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        await mqttClient.PublishAsync(new MqttApplicationMessage { Topic = "BachelorPad", Payload = Encoding.UTF8.GetBytes("Connected!"), QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce, Retain = false });
                        stopwatch.Stop();
                        DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
                    }
                    else
                    {
                        DoLog("FAIL", 3, true, 14);
                        Program.BootWithoutError = false;
                    }
                }
                catch (Exception exception)
                {
                    DoLog("ERROR", 3, true, 12);
                    Program.BootWithoutError = false;
                    LogException(exception);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                Program.BootWithoutError = false;
            }
            return;
        }
        public static async Task SendMQTTMessageAsync(string topic, string payload)
        {
            if (mqttClient.IsConnected)
            {
                try
                {
                    //Random random = new Random();
                    //int r = random.Next();
                    //DoLog($"Preparing and publishing message ", false);
                    //DoLog(r.ToString(), 3, true, 13);
                    //Stopwatch stopwatch = new Stopwatch();
                    //stopwatch.Start();
                    //await mqttClient.PublishAsync(new MqttApplicationMessage { Topic = topic, Payload = Encoding.UTF8.GetBytes(payload), QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce, Retain = false });
                    //stopwatch.Stop();
                    //DoLog($"Sending message ", false);
                    //DoLog(r.ToString(), 3, false, 13);
                    //DoLog($" the first time took {stopwatch.ElapsedMilliseconds}ms.", 2);
                    //stopwatch.Reset();

                    //stopwatch.Start();                   
                    var message = new MqttApplicationMessageBuilder()
                       .WithTopic(topic)
                       .WithPayload(payload)
                       .WithAtLeastOnceQoS()
                       .WithRetainFlag(false)
                       .Build();
                    await mqttClient.PublishAsync(message);
                    //stopwatch.Stop();
                    //DoLog($"Sending message ", false);
                    //DoLog(r.ToString(), 3, false, 13);
                    //DoLog($" the second time took {stopwatch.ElapsedMilliseconds}ms.",2);
                    return;
                }
                catch (Exception exception)
                {
                    LogException(exception);
                }

            } else
            {
                DoLog("MQTT client NOT connected to server!", 4);
            }
        }

        public static void Testing()
        {
            SendInitialData().Wait();
            //Homie.CreateAndListNodes();
            //foreach (PublishModel pm in MakeDeviceAttributes())
            //{
            //    Console.WriteLine($"{pm.PublishPath}  -->  {pm.Payload}");
            //}
        }

        private static List<PublishModel> MakeDeviceAttributes()
        {
            NetworkInterface networkInterface = NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.OperationalStatus == OperationalStatus.Up).FirstOrDefault();
            string myIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString();          

            String BaseTopic = (Program.Settings.MQTT_BASETOPIC + "/" + Program.Settings.NAME + "/").Replace("//","/");
            //Dictionary<string, string> attributes = new Dictionary<string, string>
            List<PublishModel> attributes = new List<PublishModel>
            {
                new PublishModel($"{BaseTopic}$homie", Program.Settings.MQTT_HOMIE_HOMIE ),                     //homie/super-car/$homie → "2.1.0"
                new PublishModel($"{BaseTopic}$name", Program.Settings.MQTT_HOMIE_NAME ),                       //homie/super-car/$name → "Super car"
                new PublishModel($"{BaseTopic}$localip", myIP ),                                                //homie/super-car/$localip → "192.168.0.10"
                new PublishModel($"{BaseTopic}$mac", networkInterface.GetPhysicalAddress().ToString()),         //homie/super-car/$mac → "DE:AD:BE:EF:FE:ED"
                new PublishModel($"{BaseTopic}$fw/name", Program.Settings.MQTT_HOMIE_FW_NAME),                  //homie/super-car/$fw/name → "weatherstation-firmware"
                new PublishModel($"{BaseTopic}$fw/version", Program.Settings.MQTT_HOMIE_FW_VERSION ),           //homie/super-car/$fw/version → "1.0.0"
                new PublishModel($"{BaseTopic}$nodes", GetNodeList()),                                          //homie/super-car/$nodes → "wheels,engine,lights[]"
                new PublishModel($"{BaseTopic}$implementation", Program.Settings.MQTT_HOMIE_IMPLEMENTATION ),   //homie/super-car/$implementation → "esp8266"
                new PublishModel($"{BaseTopic}$stats/interval", Program.Settings.MQTT_HOMIE_STATS_INTERVAL ),   //homie/super-car/$stats/interval → "60"
                new PublishModel($"{BaseTopic}$state", "ready" )                                                //homie/super-car/$state → "ready"
            };

            return attributes;
        }

        public async static Task SendInitialData()
        {
            try
            {
                foreach (PublishModel publishModel in MakeDeviceAttributes())
                {
                    await SendMQTTMessageAsync(publishModel.PublishPath, publishModel.Payload);
                }
                Homie.CreateAndListNodes();
                await SendNodeDataToServer();
                //return true;
            } catch (Exception exception)
            {
                MyLogger.LogException(exception);
                //return false;
            }
        }

        private static string GetNodeList()
        {
            /* There's going to be one node for every device type in use:
            * Dimmable actuators, switching actuators, push buttons, room controllers, etc
            * These will then be arrays where each datapoint is an item. */
            string nodes = "";
            List<DeviceType> activeTypes = new List<DeviceType>();
            foreach (Datapoint dp in CI.datapoints)
            {
                // Get the device type, add it to the list of active types (if it's not allready there)
                DeviceType devicetype = CI.devicetypes.Find(x => x.Number == dp.Type);
                if (!(devicetype != null && devicetype.Channels.Contains(dp.Channel) && devicetype.Modes.Contains(dp.Mode))) { continue; }
                if (!activeTypes.Contains(devicetype)) { activeTypes.Add(devicetype); }
            }

            foreach (DeviceType devType in activeTypes)
            {
                nodes += $"{ Homie.GetSafeNameForDeviceType(devType.Number) }[],";
            }
            return nodes;
        }

        //private static string GetNodes()
        //{
        //    /* There's going to be one node for every device type in use:
        //     * Dimmable actuators, switching actuators, push buttons, room controllers, etc
        //     * These will then be arrays where each datapoint is an item. */
        //    string nodes = "";
        //    List<PublishModel> pubList = new List<PublishModel>();
        //    List<DeviceType> activeTypes = new List<DeviceType>();
        //    foreach (Datapoint dp in CI.datapoints)
        //    {
        //        // Get the device type, add it to the list of active types (if it's not allready there)
        //        DeviceType devicetype = CI.devicetypes.Find(x => x.Number == dp.Type);
        //        if (!(devicetype != null && devicetype.Channels.Contains(dp.Channel) && devicetype.Modes.Contains(dp.Mode))) { continue; }
        //        if (!activeTypes.Contains(devicetype)) { activeTypes.Add(devicetype); }
        //    }

        //    foreach (DeviceType devType in activeTypes)
        //    {
        //        // Setting up the base topic string:     homie          /       supercar        / lights /
        //        string devName = Homie.GetSafeNameForDeviceType(devType.Number);
        //        string basebase = ($"{Program.Settings.MQTT_BASETOPIC}/{Program.Settings.NAME}").Replace("//", "/");
        //        string BaseTopic = ($"{basebase}/{devName}").Replace("//", "/");

        //        // Add it to the list of node names.
        //        pubList.Add(new PublishModel($"{basebase}/$nodes", $"{devName}[]"));                                            //homie/super-car/$nodes → "lights[]"
        //        nodes += $"{ devName }[],";

        //        // Get all datapoints that belong to this specific devicetype
        //        List<Datapoint> dps = new List<Datapoint>();
        //        dps.AddRange(CI.datapoints.Where(x => x.Type == devType.Number 
        //            && devType.Modes.Contains(x.Mode) 
        //            && devType.Channels.Contains(x.Channel)
        //            ));

        //        string devProps = Homie.GetPropertiesForDeviceType(devType);
        //        // Add basic info about the node
        //        pubList.Add(new PublishModel($"{BaseTopic}/$name", devName));                                                    //homie/super-car/lights/$name → "Lights"
        //        pubList.Add(new PublishModel($"{BaseTopic}/$properties", devProps));                                             //homie/super-car/lights/$properties → "intensity"
        //        pubList.Add(new PublishModel($"{BaseTopic}/$array", $"0-{dps.Count-1}"));                                        //homie/super-car/lights/$array → "0-1"

        //        foreach (string p in devProps.Replace("[]", "").Split(","))
        //        {
        //            // Each and every property must have their own set of MQTT messsages
        //            pubList.AddRange(Homie.PropertyDetails(devName, BaseTopic + "/" + p));
        //            //                                                                                                          //homie/super-car/lights/intensity/$name → "Intensity"
        //            //                                                                                                          //homie/super-car/lights/intensity/$settable → "true"
        //            //                                                                                                          //homie/super-car/lights/intensity/$unit → "%"
        //            //                                                                                                          //homie/super-car/lights/intensity/$datatype → "integer"
        //            //                                                                                                          //homie/super-car/lights/intensity/$format → "0:100"
                    
        //            int devCnt = 0;
        //            foreach (Datapoint datapoint in dps)
        //            {
        //                string dataAsString = "0";
        //                Protocol.PT_RX.Packet pT_RX = datapoint.LatestDataValues;
        //                if (datapoint.LatestDataValues != null)
        //                {
        //                    dataAsString = CI.GetDataFromPacket(pT_RX.MGW_RX_DATA, pT_RX.MGW_RX_DATA_TYPE, p);
        //                }
        //                pubList.Add(new PublishModel($"{BaseTopic}{devName}_{devCnt}/$name", datapoint.Name));                  //homie/super-car/lights_0/$name → "Back lights"
        //                pubList.Add(new PublishModel($"{BaseTopic}{devName}_{devCnt}/{p}", dataAsString));                      //homie/super-car/lights_0/intensity → "0"
        //                devCnt++;
        //            }
        //        }

        //        //foreach (PublishModel item in pubList)
        //        //{
        //        //    Console.WriteLine(item.PublishPath + "  -->  " + item.Payload);
        //        //}               
        //    }
        //    return nodes;
        //}

        private static async Task SendNodeDataToServer()
        {
            foreach (Homie.Node node in Homie.HomieNodes)
            {
                await SendMQTTMessageAsync($"{node.PublishPath}$name",$"{node.Name}");
                await SendMQTTMessageAsync($"{node.PublishPath}$properties", $"{node.Properties}");
                await SendMQTTMessageAsync($"{node.PublishPath}$array",$"{node.Array}");
            }

            foreach (Homie.Property node in Homie.HomieProperties)
            {
                await SendMQTTMessageAsync($"{node.PublishPath}$name",$"{node.Name}");
                await SendMQTTMessageAsync($"{node.PublishPath}$settable",$"{node.Settable}");
                await SendMQTTMessageAsync($"{node.PublishPath}$unit",$"{node.Unit}");
                await SendMQTTMessageAsync($"{node.PublishPath}$datatype",$"{node.DataType}");
                await SendMQTTMessageAsync($"{node.PublishPath}$format",$"{node.Format}");
                await SendMQTTMessageAsync($"{node.PublishPath}$unit",$"{node.Unit}");
            }

            foreach (Homie.ArrayElement node in Homie.HomieArrayElements)
            {
                await SendMQTTMessageAsync($"{node.PublishPath}/$name",$"{node.Name}");
                await SendMQTTMessageAsync($"{node.PublishPath}/${node.PropertyName}",$"{node.Value}");
            }
        }

        private static List<PublishModel> GetAllNodesAsPublication(string node)
        {
            List<PublishModel> pubList = new List<PublishModel>();
            List<DeviceType> activeTypes = new List<DeviceType>();
            foreach (Datapoint dp in CI.datapoints)
            {
                // Get the device type, add it to the list of active types (if it's not allready there)
                DeviceType devicetype = CI.devicetypes.Find(x => x.Number == dp.Type);
                if (!(devicetype != null && devicetype.Channels.Contains(dp.Channel) && devicetype.Modes.Contains(dp.Mode))) { continue; }
                if (!activeTypes.Contains(devicetype)) { activeTypes.Add(devicetype); }
            }

            foreach (DeviceType devType in activeTypes)
            {
                // Setting up the base topic string:     homie          /       supercar        / lights /
                string devName = Homie.GetSafeNameForDeviceType(devType.Number);
                string basebase = ($"{Program.Settings.MQTT_BASETOPIC}/{Program.Settings.NAME}").Replace("//", "/");
                string BaseTopic = ($"{basebase}/{devName}").Replace("//", "/");

                // Add it to the list of node names.
                pubList.Add(new PublishModel($"{basebase}/$nodes", $"{devName}[]"));                                            //homie/super-car/$nodes → "lights[]"
            }
            return pubList;
        }

        //private static List<PublishModel> GetPublicationsForNode(string node)
        //{

        //    // Setting up the base topic string:     homie          /       supercar        / lights /
        //    string devName = Homie.GetSafeNameForDeviceType(devType.Number);
        //    string basebase = ($"{Program.Settings.MQTT_BASETOPIC}/{Program.Settings.NAME}").Replace("//", "/");
        //    string BaseTopic = ($"{basebase}/{devName}").Replace("//", "/");

        //    // Add it to the list of node names.
        //    pubList.Add(new PublishModel($"{basebase}/$nodes", $"{devName}[]"));                                            //homie/super-car/$nodes → "lights[]"

        //}

        private void SendStats()
        {
            //$stats / uptime   Device → Controller Time elapsed in seconds since the boot of the device Yes Yes
            //$stats / signal   Device → Controller Signal strength in % Yes No
            //$stats / cputemp  Device → Controller CPU Temperature in °C Yes No
            //$stats / cpuload  Device → Controller CPU Load in %.Average of last $interval including all CPUs Yes No
            //$stats / battery  Device → Controller Battery level in % Yes No
            //$stats / freeheap Device → Controller Free heap in bytes Yes No
            //$stats / supply   Device → Controller Supply Voltage in V Yes No
            String BaseTopic = (Program.Settings.MQTT_BASETOPIC + "/" + Program.Settings.NAME + "/$stats/").Replace("//", "/");
            Dictionary<string, string> attributes = new Dictionary<string, string>
            {
                { $"{BaseTopic}uptime", GetStatsUptime() },
                //{ $"{BaseTopic}signal", GetStatsSignal() },
                { $"{BaseTopic}cputemp", GetStatsCPUtemp() },
                //{ $"{BaseTopic}cpuload", GetStatsCPUload() },
                //{ $"{BaseTopic}battery", GetStatsBattery() },
                //{ $"{BaseTopic}freeheap", GetStatsFreeHeap() },
                //{ $"{BaseTopic}supply", GetStatsSupply() },
            };
        }

        #region "Stats helpers"

        private string GetStatsUptime()
        {
            return DateTime.Now.Subtract(Program.ApplicationStart).TotalSeconds.ToString();
        }

        private string GetStatsSignal()
        {
            return "0%";
        }
        private string GetStatsCPUtemp()
        {
            try
            {
                string temp = "0";
                using (StreamReader r = new StreamReader("/sys/class/thermal/thermal_zone0/temp"))
                {
                    temp = r.ReadToEnd();
                }
                return temp;
            }
            catch (Exception exception)
            {
                return "-1";
            }
        }
        private string GetStatsCPUload()
        {
            return "0";
        }
        private string GetStatsBattery()
        {
            return "0%";
        }
        private string GetStatsFreeHeap()
        {
            return "0";
        }
        private string GetStatsSupply()
        {
            return "0";
        }
        #endregion

    }
}
