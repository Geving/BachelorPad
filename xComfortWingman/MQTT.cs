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
        private static readonly string BasePublishingTopic = Program.Settings.MQTT_BASETOPIC + "/" + Program.Settings.NAME;
        public static int PublicationCounter = 0;
        public static int ConfirmedPublications = 0;
        public static int FailedPublications = 0;
        private static MqttClientOptions clientOptions;

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

                clientOptions = new MqttClientOptions();

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

                // Assign events
                mqttClient.ApplicationMessageReceived += async (s, e) => { await MqttClient_ApplicationMessageReceived(s, e); };
                mqttClient.Connected += async (s, e) => {  await MqttClient_Connected(s, e); };
                mqttClient.Disconnected += async (s, e) => { await MqttClient_Disconnected(s, e); };

                // Connect to the MQTT broker/server
                try
                {
                    DoLog("Connecting to MQTT server...", false);
                    await mqttClient.ConnectAsync(clientOptions);
                    if (mqttClient.IsConnected)
                    {
                        DoLog("OK", 3, false, 10);
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        await SendMQTTMessageAsync("$state", "init", true);
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

        private static async Task MqttClient_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            string[] topics = e.ApplicationMessage.Topic.Split("/");
            int basePathLevels = Program.Settings.MQTT_BASETOPIC.Split("/").Length;
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
                                        await SendMQTTMessageAsync($"{topics[basePathLevels]}/{topics[basePathLevels + 1]}/result", Program.Settings.GetSettingsAsJSON(), false);
                                    }
                                    else
                                    {
                                        await SendMQTTMessageAsync($"{topics[basePathLevels]}/{topics[basePathLevels + 1]}/result", Program.Settings.WriteSettingsToFile(Encoding.UTF8.GetString(e.ApplicationMessage.Payload)).ToString(), false);
                                    }
                                    break;
                                }
                            case "datapoints":
                                {
                                    if (topics[basePathLevels] == "get")
                                    {
                                        await SendMQTTMessageAsync($"{topics[basePathLevels]}/{topics[basePathLevels + 1]}/result", CI.GetDatapointFile(), false);
                                    }
                                    else
                                    {
                                        await SendMQTTMessageAsync($"{topics[basePathLevels]}/{topics[basePathLevels + 1]}/result", CI.SetDatapointFile(Encoding.UTF8.GetString(e.ApplicationMessage.Payload)).ToString(), false);
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
                                        await SendMQTTMessageAsync($"{topics[basePathLevels]}/{topics[basePathLevels + 1]}/result", CI.ImportDatapointsOneByOne(Encoding.UTF8.GetString(e.ApplicationMessage.Payload)).ToString(), false);
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
        }

        private static async Task MqttClient_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            try
            {
                // Subscribe to a bunch of topics:
                await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic($"{Program.Settings.MQTT_BASETOPIC}/$broadcast/#").Build()); // Example: homie/$broadcast/alert ← "Intruder detected"
                await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic($"{BasePublishingTopic}/get/#").Build());    // Used for get-/setting data that's not part of the Homie specs, like
                await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic($"{BasePublishingTopic}/set/#").Build());    // config files, config and such.
                await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic($"{BasePublishingTopic}/RAW/in").Build());   // Allows the user to send raw bytes to the CI
                await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic($"{BasePublishingTopic}/raw/in").Build());   // Same as RAW, but the bytes are now human readable strings "06 C1 04 ..."
                //await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic($"{BasePublishingTopic}/shell/#").Build());  // Runs shell commands on the system. Potensial SECURITY HOLE

                while (mqttClient.IsConnected) // As long as we are connected, we need to send the stats periodically
                {
                    System.Threading.Thread.Sleep(Convert.ToInt32(Program.Settings.MQTT_HOMIE_STATS_INTERVAL) * 1000);
                    await Homie.PublishStats();
                }
            }
            catch (Exception exception)
            {
                LogException(exception);
            }
        }

        private static async Task MqttClient_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
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
            }
            catch (Exception exception)
            {
                DoLog("Reconnecting to MQTT server...", false);
                DoLog("ERROR", 3, true, 12);
                LogException(exception);
            }
        }

        public static async Task SendMQTTMessageAsync(string topic, string payload, bool retainOnServer)
        {
            await SendMQTTMessageAsync(topic, payload, retainOnServer, ++PublicationCounter);
        }

        public static async Task SendMQTTMessageAsync(string topic, string payload, bool retainOnServer, int cnt)
        {
            if (mqttClient.IsConnected)
            {
                try
                {  
                    string totaltopic = ($"{Program.Settings.MQTT_BASETOPIC}/{Program.Settings.NAME}/{topic}").Replace("//", "/");
                    //string totaltopic = ($"{Program.Settings.MQTT_BASETOPIC}/{Program.Settings.NAME}{cnt}/{topic}").Replace("//", "/");
                    //Console.WriteLine(totaltopic + "\t\t--->\t\t" + payload);
                    var message = new MqttApplicationMessageBuilder()
                       .WithTopic(totaltopic)
                       .WithPayload(payload)
                       .WithAtLeastOnceQoS()
                       .WithRetainFlag(true) // For now, force retain on all messages! //TODO: Change back to boolean when you've checked that all callers are properly configured.
                       .Build();
                    await mqttClient.PublishAsync(message);
                    ConfirmedPublications++;
                }
                catch (Exception exception)
                {
                    FailedPublications++;
                    LogException(exception);
                }
            } else
            {
                DoLog("MQTT client NOT connected to server!", 4);
            }
        }

        private async static Task PublishDeviceAttributes()
        {
            String BaseTopic = ""; // This will be added by the SendMQTTMessageAsync method...
            NetworkInterface networkInterface = NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.OperationalStatus == OperationalStatus.Up).FirstOrDefault();
            string myIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString();
            string myNodes = GetNodeList();

            await SendMQTTMessageAsync($"{BaseTopic}$homie", Program.Settings.MQTT_HOMIE_HOMIE, true);                      //homie/super-car/$homie → "2.1.0"
            await SendMQTTMessageAsync($"{BaseTopic}$name", Program.Settings.MQTT_HOMIE_NAME, true);                        //homie/super-car/$name → "Super car"
            await SendMQTTMessageAsync($"{BaseTopic}$localip", myIP, true);                                                 //homie/super-car/$localip → "192.168.0.30"
            await SendMQTTMessageAsync($"{BaseTopic}$mac", networkInterface.GetPhysicalAddress().ToString(), true);         //homie/super-car/$mac → "DE:AD:BE:EF:FE:ED"
            await SendMQTTMessageAsync($"{BaseTopic}$fw/name", Program.Settings.MQTT_HOMIE_FW_NAME, true);                  //homie/super-car/$fw/name → "weatherstation-firmware"
            await SendMQTTMessageAsync($"{BaseTopic}$fw/version", Program.Settings.MQTT_HOMIE_FW_VERSION, true);            //homie/super-car/$fw/version → "1.0.0"
            await SendMQTTMessageAsync($"{BaseTopic}$nodes", myNodes, true);                                                //homie/super-car/$nodes → "wheels,engine,lights[]"
            await SendMQTTMessageAsync($"{BaseTopic}$implementation", Program.Settings.MQTT_HOMIE_IMPLEMENTATION, true);    //homie/super-car/$implementation → "esp8266"
            await SendMQTTMessageAsync($"{BaseTopic}$stats/interval", Program.Settings.MQTT_HOMIE_STATS_INTERVAL, true);    //homie/super-car/$stats/interval → "60"
            await SendMQTTMessageAsync($"{BaseTopic}$state", "ready" , true);                                               //homie/super-car/$state → "ready"
        }

        private static List<PublishModel> MakeDeviceAttributes()
        {
            NetworkInterface networkInterface = NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.OperationalStatus == OperationalStatus.Up).FirstOrDefault();
            string myIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString();
            string myNodes = GetNodeList();

            String BaseTopic = (Program.Settings.MQTT_BASETOPIC + "/" + Program.Settings.NAME + "/").Replace("//","/");
            List<PublishModel> attributes = new List<PublishModel>
            {
                new PublishModel($"{BaseTopic}$homie", Program.Settings.MQTT_HOMIE_HOMIE ),                     //homie/super-car/$homie → "2.1.0"
                new PublishModel($"{BaseTopic}$name", Program.Settings.MQTT_HOMIE_NAME ),                       //homie/super-car/$name → "Super car"
                new PublishModel($"{BaseTopic}$localip", myIP ),                                                //homie/super-car/$localip → "192.168.0.10"
                new PublishModel($"{BaseTopic}$mac", networkInterface.GetPhysicalAddress().ToString()),         //homie/super-car/$mac → "DE:AD:BE:EF:FE:ED"
                new PublishModel($"{BaseTopic}$fw/name", Program.Settings.MQTT_HOMIE_FW_NAME),                  //homie/super-car/$fw/name → "weatherstation-firmware"
                new PublishModel($"{BaseTopic}$fw/version", Program.Settings.MQTT_HOMIE_FW_VERSION ),           //homie/super-car/$fw/version → "1.0.0"
                //new PublishModel($"{BaseTopic}$nodes", myNodes),                                          //homie/super-car/$nodes → "wheels,engine,lights[]"
                new PublishModel($"{BaseTopic}$implementation", Program.Settings.MQTT_HOMIE_IMPLEMENTATION ),   //homie/super-car/$implementation → "esp8266"
                new PublishModel($"{BaseTopic}$stats/interval", Program.Settings.MQTT_HOMIE_STATS_INTERVAL ),   //homie/super-car/$stats/interval → "60"
                //new PublishModel($"{BaseTopic}$state", "ready" )                                                //homie/super-car/$state → "ready"
            };

            if (Program.Settings.DEBUGMODE)
            {
                foreach (PublishModel pm in attributes)
                {
                    DoLog(pm.PublishPath + " --> " + pm.Payload,2,true);
                }
            }

            return attributes;
        }

        public async static Task SendInitialData()
        {
            Stopwatch stopwatch = new Stopwatch();
            //DoLog("Publishing data structures...", false);
            try
            {
                //foreach (PublishModel publishModel in MakeDeviceAttributes())
                //{
                //    await SendMQTTMessageAsync(publishModel.PublishPath, publishModel.Payload,true);
                //}

                stopwatch.Start();
                DoLog("Publishing device attributes...", false);
                await PublishDeviceAttributes();
                DoLog("OK", 3, false, 10);
                DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
                stopwatch.Reset();

                //stopwatch.Start();
                //DoLog("Publishing datapoint list...", false);
                //await SendMQTTMessageAsync("$nodes", GetNodeList(), true);
                //DoLog("OK", 3, false, 10);
                //DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
                //stopwatch.Reset();

                stopwatch.Start();
                DoLog("Publishing datapoint attributes...", false);
                foreach (Datapoint datapoint in CI.datapoints)
                {
                    await Homie.PublishDatapointAsNode(datapoint);
                }
                DoLog("OK", 3, false, 10);
                DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
                stopwatch.Stop();
            } catch (Exception exception)
            {
                MyLogger.LogException(exception);
                Program.BootWithoutError = false;
                DoLog("FAILED", 3, false, 12);
            }
            //DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
            //Program.MQTTdone = true;
        }

        private static string GetNodeList()
        {
            string nodes = "";   
            foreach (Datapoint dp in CI.datapoints)
            {
                nodes += $"{ Homie.SanitiseString(dp.Name) },";
            }
            
            if (nodes.EndsWith(',')) nodes = nodes.Remove(nodes.Length - 1);
            return nodes;
        }

        //private static string GetNodeList()
        //{
        //    /* There's going to be one node for every device type in use:
        //    * Dimmable actuators, switching actuators, push buttons, room controllers, etc
        //    * These will then be arrays where each datapoint is an item. */
        //    string nodes = "";
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
        //        nodes += $"{ Homie.GetSafeNameForDeviceType(devType.Number) },";
        //    }
        //    if (nodes.EndsWith(',')) nodes = nodes.Remove(nodes.Length - 1);
        //    //DoLog("Returning nodes: " + nodes, 4);
        //    return nodes;
        //}

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

        public static async Task PublishNode(Homie.Node node)
        {
            await SendMQTTMessageAsync($"{node.PublishPath}/$name", $"{node.Name}", true);
            await SendMQTTMessageAsync($"{node.PublishPath}/$properties", $"{node.Properties}", true);
            await SendMQTTMessageAsync($"{node.PublishPath}/$array", $"{node.Array}", true);
        }

        public static async Task PublishProperty(Homie.Property node)
        {
            if (node.Name != "") await SendMQTTMessageAsync($"{node.PublishPath}/$name", $"{node.Name}", true);
            if (node.Settable != "") await SendMQTTMessageAsync($"{node.PublishPath}/$settable", $"{node.Settable}", true);
            if (node.Unit != "") await SendMQTTMessageAsync($"{node.PublishPath}/$unit", $"{node.Unit}", true);
            if (node.DataType != "") await SendMQTTMessageAsync($"{node.PublishPath}/$datatype", $"{node.DataType}", true);
            if (node.Format != "") await SendMQTTMessageAsync($"{node.PublishPath}/$format", $"{node.Format}", true);
            if (node.Unit != "") await SendMQTTMessageAsync($"{node.PublishPath}/$unit", $"{node.Unit}", true);
        }

        public static async Task PublishArrayElement(Homie.ArrayElement node)
        {
            await SendMQTTMessageAsync($"{node.PublishPath}/$name", $"{node.Name}", true);
            await SendMQTTMessageAsync($"{node.PublishPath}/${node.PropertyName}", $"{node.Value}", true);
        }

        private static async Task SendNodeDataToServer()
        {
            foreach (Homie.Node node in Homie.HomieNodes)
            {
                await PublishNode(node);
            }

            foreach (Homie.Property node in Homie.HomieProperties)
            {
                await PublishProperty(node);
            }

            foreach (Homie.ArrayElement node in Homie.HomieArrayElements)
            {
                await PublishArrayElement(node);
            }
        }

        //////private static List<PublishModel> GetAllNodesAsPublication(string node)
        //////{
        //////    List<PublishModel> pubList = new List<PublishModel>();
        //////    List<DeviceType> activeTypes = new List<DeviceType>();
        //////    foreach (Datapoint dp in CI.datapoints)
        //////    {
        //////        // Get the device type, add it to the list of active types (if it's not allready there)
        //////        DeviceType devicetype = CI.devicetypes.Find(x => x.Number == dp.Type);
        //////        if (!(devicetype != null && devicetype.Channels.Contains(dp.Channel) && devicetype.Modes.Contains(dp.Mode))) { continue; }
        //////        if (!activeTypes.Contains(devicetype)) { activeTypes.Add(devicetype); }
        //////    }

        //////    foreach (DeviceType devType in activeTypes)
        //////    {
        //////        // Setting up the base topic string:     homie          /       supercar        / lights /
        //////        string devName = Homie.GetSafeNameForDeviceType(devType.Number);
        //////        string basebase = ($"{Program.Settings.MQTT_BASETOPIC}/{Program.Settings.NAME}").Replace("//", "/");
        //////        string BaseTopic = ($"{basebase}/{devName}").Replace("//", "/");

        //////        // Add it to the list of node names.
        //////        pubList.Add(new PublishModel($"{basebase}/$nodes", $"{devName}XX[]"));                                            //homie/super-car/$nodes → "lights[]"
        //////    }
        //////    return pubList;
        //////}

        //private static List<PublishModel> GetPublicationsForNode(string node)
        //{

        //    // Setting up the base topic string:     homie          /       supercar        / lights /
        //    string devName = Homie.GetSafeNameForDeviceType(devType.Number);
        //    string basebase = ($"{Program.Settings.MQTT_BASETOPIC}/{Program.Settings.NAME}").Replace("//", "/");
        //    string BaseTopic = ($"{basebase}/{devName}").Replace("//", "/");

        //    // Add it to the list of node names.
        //    pubList.Add(new PublishModel($"{basebase}/$nodes", $"{devName}[]"));                                            //homie/super-car/$nodes → "lights[]"

        //}

        

        public async static Task PublishDeviceData(Datapoint datapoint)
        {
            // Get the correct device type for the datapoint
            DeviceType devicetype = CI.devicetypes.Find(x => x.Number == datapoint.Type && x.Channels.Contains(datapoint.Channel) && x.Modes.Contains(datapoint.Mode));

            // Count all datapoints that belong to this specific devicetype
            //int devcount = CI.datapoints.Where(x => x.Type == devicetype.Number && devicetype.Modes.Contains(x.Mode) && devicetype.Channels.Contains(x.Channel)).Count();
            List<Datapoint> dps = new List<Datapoint>(CI.datapoints.Where(x => x.Type == devicetype.Number && devicetype.Modes.Contains(x.Mode) && devicetype.Channels.Contains(x.Channel)).OrderBy(x => x.DP));
            int devcount = dps.Count();

            int dpindex = dps.FindIndex(dp => dp.DP == datapoint.DP);
                       
            // Add the properties for this node/device
            string devProps = Homie.GetPropertiesForDeviceType(devicetype);
            string BaseTopic = Program.Settings.MQTT_BASETOPIC + "/" + Program.Settings.NAME;
            string devName = datapoint.Name;
            Homie.Node node = new Homie.Node { PublishPath = BaseTopic, Name = devName, Array = $"0-{devcount - 1}", Properties = devProps };
                       
            foreach (string p in devProps.Replace("[]", "").Split(","))
            {
                // Get properties for the device type. (This will unfortunately be repeated for each device, as we don't know if this is the first device of its type.
                Homie.Property property = Homie.GetHomiePropertyFor(devName, BaseTopic + "/" + p);

                string dataAsString = "0";
                // If the datapoint has data already attached to it, use that!
                if (datapoint.LatestDataValues != null) dataAsString = CI.GetDataFromPacket(datapoint.LatestDataValues.MGW_RX_DATA, datapoint.LatestDataValues.MGW_RX_DATA_TYPE, p);

                Homie.ArrayElement homieArrayElement = new Homie.ArrayElement { Name = datapoint.Name, PropertyName = p, Value = dataAsString, PublishPath = $"{BaseTopic}/{devName}_{devcount}", BelongsToDP = datapoint.DP, ArrayIndex = devcount };
                await MQTT.PublishProperty(property);
                await MQTT.PublishArrayElement(homieArrayElement);
            }
        }




    }
}
