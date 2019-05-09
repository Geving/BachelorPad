using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using static xComfortWingman.MyLogger;

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
                    int basePathLevels = settings.MQTT_BASEPATH.Split("/").Length;
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
                    }
                }
                catch (Exception exception)
                {
                    DoLog("ERROR", 3, true, 12);
                    LogException(exception);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
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


    }
}
