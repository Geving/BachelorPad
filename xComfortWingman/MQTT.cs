//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
//using MQTTnet;
//using MQTTnet.Client;
//using MQTTnet.Protocol;

//namespace xComfortWingman
//{
//    public static class MQTT
//    {
//        public static async Task RunAsync()
//        {
//            try
//            {
//                // Create a new MQTT client.
//                var factory = new MqttFactory();
//                mqttClient = factory.CreateMqttClient();
//                var clientOptions = new MqttClientOptions
//                {
//                    ChannelOptions = new MqttClientTcpOptions
//                    {
//                        Server = "192.168.0.3"
//                    }
//                };

//                // Create TCP based options using the builder.
//                var options = new MqttClientOptionsBuilder()
//                    .WithClientId("Client1")
//                    .WithTcpServer("192.168.0.3")
//                    //.WithCredentials("mySonoff", "pusur")
//                    //.WithTls()
//                    .WithCleanSession()
//                    .Build();

//                // Use WebSocket connection.
//                //var options = new MqttClientOptionsBuilder()
//                //    .WithWebSocketServer("192.168.0.3:1883/mqtt")
//                //    .Build();

//                await mqttClient.ConnectAsync(clientOptions);

//                await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("BachelorPad/xComfort/cmd").Build());

//                //var message = new MqttApplicationMessageBuilder()
//                //    .WithTopic("BachelorPad/xComfort/cmd")
//                //    .WithPayload("Hello World!")
//                //    .WithExactlyOnceQoS()
//                //    .WithRetainFlag(false)
//                //    .Build();
//                //await mqttClient.PublishAsync(message);



//                mqttClient.ApplicationMessageReceived += (s, e) =>
//                {
//                    Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
//                    Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
//                    Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
//                    Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
//                    Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
//                    Console.WriteLine();
//                };


//                mqttClient.Connected += async (s, e) =>
//                {
//                    Console.WriteLine("### CONNECTED WITH SERVER ###");

//                    // Subscribe to a topic
//                    await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("my/topic").Build());

//                    Console.WriteLine("### SUBSCRIBED ###");
//                };

//                mqttClient.Disconnected += async (s, e) =>
//                {
//                    Console.WriteLine("### DISCONNECTED FROM SERVER ###");
//                    await Task.Delay(TimeSpan.FromSeconds(5));

//                    try
//                    {
//                        await mqttClient.ConnectAsync(clientOptions);
//                    }
//                    catch
//                    {
//                        Console.WriteLine("### RECONNECTING FAILED ###");
//                    }
//                };

//                try
//                {
//                    await mqttClient.ConnectAsync(clientOptions);
//                }
//                catch (Exception exception)
//                {
//                    Console.WriteLine("### CONNECTING FAILED ###" + Environment.NewLine + exception);
//                }

//                Console.WriteLine("### WAITING FOR APPLICATION MESSAGES ###");

//                await mqttClient.SubscribeAsync(new TopicFilter("test", MqttQualityOfServiceLevel.AtMostOnce));


//                Console.WriteLine("Done connecting to MQTT server...");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//            }
//        }

//    }
//}
