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
        private static readonly string BasePublishingTopic = Program.Settings.MQTT_BASETOPIC + "/" + Program.Settings.GENERAL_NAME;
        public static Int64 PublicationCounter = 0;
        public static Int64 ConfirmedPublications = 0;
        public static Int64 FailedPublications = 0;
        public static int ReconnectionFailures = 0;
        private static MqttClientOptions clientOptions;
        public static bool AutoReconnect = false;
        public static bool UseHomie = Program.Settings.HOMIE_USE_HOMIE;
        public static bool UseBasic = Program.Settings.BASIC_USE_BASIC;
        public static bool UseHomeAssistant = Program.Settings.HOMEASSISTANT_USE_HOMEASSISTANT;

        public static async Task RunMQTTClientAsync()
        {
            try
            {
                AutoReconnect = true;
                Settings settings = Program.Settings;
                var factory = new MqttFactory();
                mqttClient = factory.CreateMqttClient();

                var ClientID = settings.MQTT_CLIENT_ID.Replace("%rnd%", new Guid().ToString());

                var WillMessage = new MqttApplicationMessageBuilder()
                       .WithTopic((BasePublishingTopic + "/$state").Replace("//","/"))
                       .WithPayload("lost")
                       .WithAtLeastOnceQoS()
                       .WithRetainFlag(false)
                       .Build();

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

                if (settings.MQTT_CONNECTION_METHOD == MQTT_CONNECTION_METHOD.TCP)
                {
                    clientOptions = new MqttClientOptions
                    {
                        CleanSession = settings.MQTT_CLEAN_SESSION,
                        ClientId = ClientID,
                        Credentials = Credentials,
                        ChannelOptions = ChannelOptions_TCP,
                        CommunicationTimeout = TimeSpan.FromSeconds(3),
                        WillMessage=WillMessage
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
                        CommunicationTimeout = TimeSpan.FromSeconds(3),
                        WillMessage=WillMessage
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
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    await mqttClient.ConnectAsync(clientOptions);
                    if (mqttClient.IsConnected)
                    {
                        DoLog("OK", 3, false, 10);
                        //await SendMQTTMessageAsync("$state", "init", true);
                        stopwatch.Stop();
                        DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
                        
                        //if (UseHomeAssistant)
                        //{
                        //    stopwatch.Start();
                        //    HomeAssistant.Heartbeat();
                        //    stopwatch.Stop();
                        //    DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
                        //}

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

        public static async Task DisconnectMQTTClientAsync()
        {
            // Send disconnected message for each device...
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            DoLog("Updating state for all devices...", false);

            if (UseHomeAssistant)
            {
                HomeAssistant.Heartbeat("offline");
            }

            if (UseHomie)
            {
                foreach (Homie.Device device in Homie.devices)
                {
                    device.State = "disconnected";
                    await SendMQTTMessageAsync($"homie/{device.Name}/$state", device.State, true);
                }
            }

            if (UseBasic)
            {

            }
            DoLog("OK", 3, false, 10);
            DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
            stopwatch.Reset();

            AutoReconnect = false; // We don't want to reconnect automatically...
            await mqttClient.DisconnectAsync(); // Disconnect the MQTT client from the broker
        }
   
        private static async Task MqttClient_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            try
            {
                ReconnectionFailures = 0; //Reset errorcounter

                // Suscribe to our default topics
                List<string> topics = new List<string>()
                 {
                    $"{Program.Settings.MQTT_BASETOPIC}/$broadcast/#",      // Example: homie/$broadcast/alert ← "Intruder detected"
                    $"{BasePublishingTopic}/get/",                          // Used for get-/setting data that's not part of the Homie specs, like
                    $"{BasePublishingTopic}/set/",                          // config files, config and such.
                    $"{BasePublishingTopic}/RAW/in/",                       // Allows the user to send raw bytes to the CI
                    $"{BasePublishingTopic}/raw/in/",                       // Same as RAW, but the bytes are now human readable strings "06 C1 04 ..."
                    $"{BasePublishingTopic}/cmd/",                          // A way to receive simple commands like "exit" 
                    $"{Program.Settings.MQTT_BASETOPIC}/ClearAutoConfig"    // Nasty...
                    //$"{BasePublishingTopic}/shell/#"                      // Runs shell commands on the system. Potensial SECURITY HOLE
                };

                if (UseHomeAssistant)
                {
                    foreach (HomeAssistant.Device device in HomeAssistant.deviceList)
                    {
                        //DoLog(device.devtopic.Substring(HomeAssistant.BaseTopic.Length + 1) + "/config", true);
                        if (device.ReadOnly == false)
                        {
                            topics.Add($"{device.devtopic}/set");
                        }
                        //Send Auto Config on every boot?
                        //await SendMQTTMessageAsync(device.Config_topic + "/config", device.ConfigString, false);
                    }
                }

                if (UseHomie)
                {
                    // Time to tell everyone about our devices, and set up a subscription for that device if needed.
                    foreach (Homie.Device device in Homie.devices)
                    {
                        await MQTT.PublishHomieDeviceAsync(device);                  // Publishes a LOT of information
                        foreach (Homie.Node node in device.Node)
                        {
                            foreach (Homie.Property property in node.PropertyList)
                            {
                                if (property.Settable == "true")                // If this is a settable value, we need to subscribe to a topic for it
                                {
                                    //await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic($"{Program.Settings.MQTT_BASETOPIC}/{device.Name}/{node.PathName}/{property.PathName}/set".Replace("//","/")).Build());
                                    topics.Add($"{Program.Settings.MQTT_BASETOPIC}/{device.Name}/{node.PathName}/{property.PathName}/set");
                                }
                                if (device.Datapoint.Class == 0)                // If this is a pollable value, we need to subscribe to a topic for that as well
                                {
                                    //await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic($"{Program.Settings.MQTT_BASETOPIC}/{device.Name}/{node.PathName}/{property.PathName}/poll".Replace("//", "/")).Build()); 
                                    topics.Add($"{Program.Settings.MQTT_BASETOPIC}/{device.Name}/{node.PathName}/{property.PathName}/poll"); // This will allow us to request an update from the device
                                }

                            }
                        }
                    }
                }

                foreach (string unsafetopic in topics)
                {
                    string topic = unsafetopic.Replace("//", "/");          // Depending on what the user has put in the settings file, this might contain //, so we remove them just in case.
                    await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(topic).Build());
                    if(topic.EndsWith("/")) { await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(topic.Remove(topic.Length-1)).Build()); } // This allows us to subscribe to both topics at once.
                    if (Program.Settings.GENERAL_DEBUGMODE) DoLog($"Subscribing to {topic}", 2);
                }

                int BeatCount = 0;
                while (mqttClient.IsConnected && Program.StayAlive) // As long as we are connected, we need to send the stats periodically
                {
                    if (UseHomeAssistant) { 
                        HomeAssistant.Heartbeat(); 
                        if (BeatCount++ >= 10)
                        {
                            HomeAssistant.SendAutoConfig();
                            BeatCount = 0;
                        }
                    }
                    System.Threading.Thread.Sleep(Convert.ToInt32(Program.Settings.HOMIE_STATS_INTERVAL) * 1000);
                    //await Homie.PublishStats();
                }
            }
            catch (Exception exception)
            {
                LogException(exception);
            }
        }
      
        private static async Task MqttClient_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            // These are the topics we are subscribing to:
            //$"homie/DimKitchen/Lights/DimLevel/set"                               // Used to set a new value for that node/property
            //$"homie/DimKitchen/Lights/DimLevel/poll"                              // This will allow us to request an update from the device
            //$"homie/$broadcast/#"                                                 // Example: homie/$broadcast/alert ← "Intruder detected"
            //$"homie/xComfort/get"                                                                    // Used for get-/setting data that's not part of the Homie specs, like
            //$"homie/xComfort/set"                                                                    // config files, config and such.
            //$"homie/xComfort/RAW/in"                                                                   // Allows the user to send raw bytes to the CI
            //$"homie/xComfort/raw/in"                                                                   // Same as RAW, but the bytes are now human readable strings "06 C1 04 ..."
            //$"homie/xComfort/cmd"                                                                    // A way to receive simple commands like "exit" 
            //$"homie/xComfort/shell"                                                                  // Runs shell commands on the system. Potensial SECURITY HOLE

            // Technically, it's these formats, but they are not as nice to read:
            //$"{Program.Settings.MQTT_BASETOPIC}/{device.Name}/{node.PathName}/{property.PathName}/set" (and poll)
            //$"{Program.Settings.MQTT_BASETOPIC}/$broadcast/#"
            //$"{BasePublishingTopic}/get/#" (and set/# and RAW/in and raw/in and cmd/# and shell/#)

           

            string[] topics = e.ApplicationMessage.Topic.Split("/"); // Split the topic into levels
            string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload); // e.ApplicationMessage.ConvertPayloadToString();

            if (Program.Settings.GENERAL_DEBUGMODE) DoLog($"Incomming MQTT message: {e.ApplicationMessage.Topic}={payload}",2);

            // (We trust that the MQTT subscription makes sure that we don't get any unwanted suff, so we don't have to check that ourselves.)
            switch (topics[1]) // This will always be "$broadcast", "xComfort" or the name of a device.
            {
                case "$broadcast":      // It's a broadcast message for all devices
                    {
                        // We have no real use for this at the moment, but we'll display and log it anyway.
                        DoLog($"Broadcasted message: {e.ApplicationMessage.Topic.ToString()}={payload}", 4);
                        break;
                    }
                case "xComfort":    // It's one of six possible things
                    {
                        switch (topics[2])
                        {
                            case "get":
                                {
                                    if (topics.Length > 3 && topics[3] == "result") { DoLog("MQTT bug workaround!",2); break; } // A bug in the library. It doesn't properly unsubscribe to topics...
                                    // Get and settable
                                    if (payload=="config") await SendMQTTMessageAsync($"{topics[1]}/{topics[2]}/result", Program.Settings.GetSettingsAsJSON(), false);
                                    if (payload == "datapoints") await SendMQTTMessageAsync($"{topics[1]}/{topics[2]}/result", Program.GetDatapointFile(), false);
                                    if (payload == "debug") await SendMQTTMessageAsync($"{topics[1]}/{topics[2]}/result", Program.Settings.GENERAL_DEBUGMODE.ToString(), false);

                                    // Gettable only
                                    if (payload == "temperature") await SendMQTTMessageAsync($"{topics[1]}/{topics[2]}/result", Homie.GetStatsCPUload(), false);
                                    if (payload == "status")
                                    {
                                        string reply = "--- STAUTS ---\n";
                                        reply += $"Debug mode: {Program.Settings.GENERAL_DEBUGMODE}\n";
                                        reply += $"StayAlive: {Program.StayAlive}\n";
                                        reply += $"Datapoints: {CI.datapoints.Count}\n";
                                        reply += $"Devices: {Homie.devices.Count}\n";
                                        reply += $"Temperature: {Homie.GetStatsCPUtemp()}";
                                        reply += $"Publications: {PublicationCounter}\n";
                                        reply += "--- EOT ---\n";
                                        await SendMQTTMessageAsync($"{topics[1]}/{topics[2]}/result", reply, false);
                                    }
                                    break;
                                }
                            case "set":
                                {
                                    if (topics.Length > 3 && topics[3] == "result") { DoLog("MQTT bug workaround!", 2); break; } // A bug in the library. It doesn't properly unsubscribe to topics...
                                    // Get and settable
                                    if (payload == "config") await SendMQTTMessageAsync($"{topics[1]}/{topics[2]}/result", Program.Settings.WriteSettingsToFile(payload).ToString(), false);
                                    if (payload == "datapoints") await SendMQTTMessageAsync($"{topics[1]}/{topics[2]}/result", Program.SetDatapointFile(payload).ToString(), false);
                                    if (payload == "debug")
                                    {
                                        Program.Settings.GENERAL_DEBUGMODE = (payload == "true");
                                        await SendMQTTMessageAsync($"{topics[1]}/{topics[2]}/result", Program.Settings.GENERAL_DEBUGMODE.ToString(), false);
                                    }

                                    // Settable only
                                    if (payload == "datapoint") await SendMQTTMessageAsync($"{topics[1]}/{topics[2]}/result", Program.ImportDatapointsOneByOne(payload).ToString(), false);
                                    
                                    break;
                                }
                            case "RAW":
                                {
                                    await CI.SendData(e.ApplicationMessage.Payload);
                                    break;
                                }
                            case "raw":
                                {
                                    // Convert the human readable string back into the actualy bytes it represents, then sending it to the CI.
                                    payload = payload.Replace(" ", "");
                                    List<byte> byteList = new List<byte>();
                                    for (int i = 0; i < payload.Length; i += 2)
                                    {
                                        byteList.Add(Convert.ToByte(payload.Substring(i, 2), 16));
                                    }
                                    await CI.SendData(byteList.ToArray());
                                    break;
                                }
                            case "cmd":
                                {
                                    if (payload == "exit") Program.StayAlive = false;
                                    if (payload == "ClearAutoConfig") HomeAssistant.ClearAutoConfig();
                                    if (payload == "SendAutoConfig") HomeAssistant.SendAutoConfig();
                                    if (payload == "ReloadAutoConfig") HomeAssistant.ReloadAutoConfig();
                                    if (payload == "update")
                                    {
                                        DoLog("Re-publishing all devices...", false);
                                        foreach (Homie.Device device in Homie.devices)
                                        {
                                            await MQTT.PublishHomieDeviceAsync(device);
                                        }
                                        DoLog("Done", 3, true, 10);
                                    }
                                    if (payload == "pollall")
                                    {
                                        DoLog("Polling all relevant devices...", true);
                                        foreach (Homie.Device device in Homie.devices)
                                        {
                                            if (device.Datapoint.Class == 0)
                                            {
                                                await CI.RequestUpdateAsync(device.Datapoint.DP);
                                                //System.Threading.Thread.Sleep(500);
                                            }
                                        }
                                        DoLog("Polling complete!", 3, true, 10);
                                    }
                                    break;
                                }
                            case "shell":
                                {
                                    // This is just a tad too much of a security risk to implement at this time.
                                    // It might be a very useful feature, but there must be some security in place first!
                                    // Have a look at https://loune.net/2017/06/running-shell-bash-commands-in-net-core/ for en example of how the execution can be implemented.
                                    DoLog("Ignored request for shell: " + payload, 3);
                                    break;
                                }
                            default: { break; }
                        }
                        break;
                    }
                default:    // It's a device's name, we must be more dynamic in our approach.
                    {
                        try
                        {
                            if (UseHomeAssistant)
                            {
                                if (Program.Settings.GENERAL_DEBUGMODE) DoLog($"Processing as datapoint related", 2);
                                if (Program.Settings.GENERAL_DEBUGMODE) DoLog($"Looking for device '" + topics[3] + "'...", 2);
                                HomeAssistant.Device device = HomeAssistant.deviceList.Find(x => x.Name == topics[3]);
                                if (Program.Settings.GENERAL_DEBUGMODE) DoLog($"...found device: {device.Name}", 2);
                                switch (topics[4])
                                {
                                    case "set": {
                                            Dictionary<string, string> payloadAttributes = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(payload);
                                            if (device.devtype == "Light" && payloadAttributes.ContainsKey("brightness"))
                                            {
                                                await CI.SendNewValueToDatapointAsync(device.DP, Convert.ToDouble(payloadAttributes["brightness"]));
                                            }
                                            else
                                            {
                                                await CI.SendNewValueToDatapointAsync(device.DP, Convert.ToDouble(payloadAttributes["state"].Replace("ON","100").Replace("OFF","0")));
                                            }
                                            break; 
                                        }
                                    //case "poll": { await CI.RequestUpdateAsync(device.DP); break; }
                                    default: { DoLog($"Unknown action: {e.ApplicationMessage.Topic}"); break; }
                                }
                            }

                            if (UseHomie)
                            {
                                if (Program.Settings.GENERAL_DEBUGMODE) DoLog($"Processing as datapoint related", 2);
                                Homie.Device device = Homie.devices.Find(x => x.Name == topics[1]);
                                if (Program.Settings.GENERAL_DEBUGMODE) DoLog($"...found device: {device.Name}", 2);
                                Homie.Node node = device.Node.Find(x => x.PathName == topics[2]);
                                if (Program.Settings.GENERAL_DEBUGMODE) DoLog($"...found node: {node.Name}", 2);
                                Homie.Property property = node.PropertyList.Find(x => x.PathName == topics[3]);
                                if (Program.Settings.GENERAL_DEBUGMODE) DoLog($"...found property: {property.Name}", 2);
                                switch (topics[4])
                                {
                                    case "set": { await CI.SendNewValueToDatapointAsync(device.Datapoint.DP, Convert.ToDouble(payload)); break; } //await Homie.UpdateSingleProperty($"{device.Name}/{node.PathName}/{property.PathName}", property, payload); 
                                    case "poll": { await CI.RequestUpdateAsync(device.Datapoint.DP); break; }
                                    default: { DoLog($"Unknown action: {e.ApplicationMessage.Topic}"); break; }
                                }
                            }
                            //if (topics[4] == "set") { await Homie.UpdateSingleProperty($"{device.Name}/{node.PathName}/{property.PathName}",property, payload); }
                            //if (topics[4] == "poll") { await CI.RequestUpdateAsync(device.Datapoint.DP); }
                            //if (topics[4] != "set" && topics[4] != "poll") { DoLog($"Unknown action: {e.ApplicationMessage.Topic}"); }
                        }
                        catch (Exception exception)
                        {
                            DoLog($"Error processing MQTT message: {e.ApplicationMessage.Topic.ToString()}={payload}",5);
                            LogException(exception);
                        }
                        break;
                    }
            }
        }

        private static async Task MqttClient_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            if (AutoReconnect)
            {
                int delayTime = 5; // Startng out with a 5 second delay before reconnecting.
                if (ReconnectionFailures >= 12) delayTime = 60; // With 12 or more consecutive errors (total of at least 1 minute), wait a full minute between attempts.
                if (ReconnectionFailures >= 21) delayTime = 600; // With 21 or more consecutive errors (total of at least 10 minutes), increase to maximum wait time of 10 minutes between attempts.
                await Task.Delay(TimeSpan.FromSeconds(delayTime));
                try
                {
                    await mqttClient.ConnectAsync(clientOptions);
                    if (mqttClient.IsConnected)
                    {
                        //DoLog("OK", 3, true, 10);
                    }
                    else
                    {
                        ReconnectionFailures++;
                        DoLog("Reconnecting to MQTT server...", false);
                        DoLog("FAIL", 3, true, 14);
                    }
                }
                catch (Exception exception)
                {
                    ReconnectionFailures++;
                    DoLog("Reconnecting to MQTT server...", false);
                    DoLog("ERROR", 3, true, 12);
                    LogException(exception);
                }
            }
        }

        public static async Task SendMQTTMessageAsync(string topic, string payload, bool retainOnServer)
        {
            await SendMQTTMessageAsync(topic, payload, retainOnServer, ++PublicationCounter);
        }

        public static async Task SendMQTTMessageAsync(string topic, string payload, bool retainOnServer, Int64 cnt)
        {
            if (mqttClient.IsConnected)
            {
                try
                {
                    string totaltopic = ($"{Program.Settings.MQTT_BASETOPIC}/{topic}").Replace("//", "/");
                    //if (UseHomeAssistant && topic.EndsWith("/config"))
                    //{
                    //    totaltopic = ($"homeassistant/{topic}").Replace("//", "/");
                    //    retainOnServer = true;
                    //}

                    //Console.WriteLine(totaltopic + "\t\t--->\t\t" + payload);
                    var message = new MqttApplicationMessageBuilder()
                       .WithTopic(totaltopic)
                       .WithPayload(payload)
                       .WithAtLeastOnceQoS()
                       .WithRetainFlag(retainOnServer)
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

        public async static Task SendInitialDataAsync()
        {
            Stopwatch stopwatch = new Stopwatch();
            //DoLog("Publishing data structures...", false);
            try
            {
                //stopwatch.Start();
                //DoLog("Creating devices from datapoints...", false);
                //foreach (Datapoint datapoint in CI.datapoints)
                //{
                //    Homie.devices.Add(Homie.GetDeviceFromDatapoint(datapoint));
                //}
                //DoLog("OK", 3, false, 10);
                //DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
                //stopwatch.Reset();
                if (UseHomie)
                {
                    stopwatch.Start();
                    DoLog("Publishing all device information...", false);
                    foreach (Homie.Device device in Homie.devices)
                    {
                        await SendMQTTMessageAsync($"homie/{device.Name}/$state", device.State, true);
                    }
                    DoLog("OK", 3, false, 10);
                    DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
                    stopwatch.Reset();
                }

                //foreach (PublishModel publishModel in MakeDeviceAttributes())
                //{
                //    await SendMQTTMessageAsync(publishModel.PublishPath, publishModel.Payload,true);
                //}

                //stopwatch.Start();
                //DoLog("Publishing device attributes...", false);
                //await PublishDeviceAttributes();
                //DoLog("OK", 3, false, 10);
                //DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
                //stopwatch.Reset();

                ////stopwatch.Start();
                ////DoLog("Publishing datapoint list...", false);
                ////await SendMQTTMessageAsync("$nodes", GetNodeList(), true);
                ////DoLog("OK", 3, false, 10);
                ////DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
                ////stopwatch.Reset();

                //stopwatch.Start();
                //DoLog("Publishing datapoint attributes...", false);
                //foreach (Datapoint datapoint in CI.datapoints)
                //{
                //    await Homie.PublishDatapointAsNode(datapoint);
                //}
                //DoLog("OK", 3, false, 10);
                //DoLog($"{stopwatch.ElapsedMilliseconds}ms", 3, true, 14);
                //stopwatch.Stop();
            }
            catch (Exception exception)
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

        //public static async Task PublishHomeAssistantDeviceAsync(HomeAssistant.Device device)
        //{
        //    try 
        //    {
        //        DoLog("Publishing Auto Config for " + device.Name, true);
        //        await SendMQTTMessageAsync(device.Config_topic, device.ConfigString, false);
        //        //await SendMQTTMessageAsync(device.State_topic, device.currentState, device.Retain);
        //    }
        //    catch (Exception exception)
        //    {
        //        Console.WriteLine(exception.Message);
        //    }

        //}

        //public static async Task PublishUpdateForHomeAssistantDeviceAsync(HomeAssistant.Device device)
        //{
        //    try
        //    {
        //        DoLog("Publishing state for " + device.Name, true);
        //        await SendMQTTMessageAsync("homie/" + device.State_topic, device.currentState, device.Retain);
        //    }
        //    catch (Exception exception)
        //    {
        //        Console.WriteLine(exception.Message);
        //    }
        //}

        public static async Task PublishHomieDeviceAsync(Homie.Device device)
            {
                try
                {

                    bool r = true;
                //                                                                                                                                DEVICE

                //await SendMQTTMessageAsync($"homie/{device.Name}/$homie", device.Homie, r);                                                     //homie/DimKitchen/$homie			              =   3.0.1	
                //await SendMQTTMessageAsync($"homie/{device.Name}/$name", device.Name, r);                                                       //homie/DimKitchen/$name			              =   Kitchen lights	
                //await SendMQTTMessageAsync($"homie/{device.Name}/$localip", device.Localip, r);                                                 //homie/DimKitchen/$localip			          =   0.0.0.0	
                //await SendMQTTMessageAsync($"homie/{device.Name}/$mac", device.Mac, r);                                                         //homie/DimKitchen/$mac				          =   00:00:00:00:00:00
                //await SendMQTTMessageAsync($"homie/{device.Name}/$fw/name", device.Fw_name, r);                                                 //homie/DimKitchen/$fw/name			          =   BachelorPad
                //await SendMQTTMessageAsync($"homie/{device.Name}/$fw/version", device.Fw_version, r);                                           //homie/DimKitchen/$fw/version		          =   1.0.0	
                //await SendMQTTMessageAsync($"homie/{device.Name}/$nodes", device.Nodes, r);                                                     //homie/DimKitchen/$nodes			              =   lights,signal	
                //await SendMQTTMessageAsync($"homie/{device.Name}/$implementation", device.Implementation, r);                                   //homie/DimKitchen/$implementation	          =   xComfort
                //await SendMQTTMessageAsync($"homie/{device.Name}/$interval", device.Stats_interval, r);                                         //homie/DimKitchen/$stats/interval	          =   60
                //await SendMQTTMessageAsync($"homie/{device.Name}/$state", device.State, r);                                                     //homie/DimKitchen/$state			              =   ready


                foreach (Homie.Node node in device.Node)
                    {
                        //                                                                                                                                       NODE

                        await SendMQTTMessageAsync($"homie/{device.Name}/{node.PathName}", node.Value, r);                                          //homie/DimKitchen/lights                       =   42

                        //await SendMQTTMessageAsync($"homie/{device.Name}/{node.PathName}/$name", node.Name, r);                                     //homie/DimKitchen/lights/$name                 =   Lights
                        //await SendMQTTMessageAsync($"homie/{device.Name}/{node.PathName}/$type", node.Type, r);                                     //homie/DimKitchen/lights/$type                 =   Dimmer
                        //await SendMQTTMessageAsync($"homie/{device.Name}/{node.PathName}/$properties", node.Properties, r);                         //homie/DimKitchen/lights/$properties           =   intensity

                    foreach (Homie.Property property in node.PropertyList)
                        {
                        //                                                                                                                                          PROPERTY
                        //await SendMQTTMessageAsync($"homie/{device.Name}/{node.PathName}/{property.PathName}/$name", property.Name, r);         //homie/DimKitchen/lights/intensity/$name       =   "Light Intensity"
                        //await SendMQTTMessageAsync($"homie/{device.Name}/{node.PathName}/{property.PathName}/$settable", property.Settable, r); //homie/DimKitchen/lights/intensity/$settable   =   "true"
                        //await SendMQTTMessageAsync($"homie/{device.Name}/{node.PathName}/{property.PathName}/$unit", property.Unit, r);         //homie/DimKitchen/lights/intensity/$unit       =   "%"
                        //await SendMQTTMessageAsync($"homie/{device.Name}/{node.PathName}/{property.PathName}/$datatype", property.DataType, r); //homie/DimKitchen/lights/intensity/$datatype   =   "integer"
                        //await SendMQTTMessageAsync($"homie/{device.Name}/{node.PathName}/{property.PathName}/$format", property.Format, r);     //homie/DimKitchen/lights/intensity/$format     =   "0:100"

                        await SendMQTTMessageAsync($"homie/{device.Name}/{node.PathName}/{property.PathName}", property.DataValue, r);
                        }
                    }
                } catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }
            }
}
