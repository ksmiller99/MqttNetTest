using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Diagnostics;
using MQTTnet.ManagedClient;
using MQTTnet.Protocol;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTPublisherTest
{
    internal class Publisher
    {
        static void Main()
        {
            Thread publisher = new Thread(async () =>
            {
                // Setup and start a managed MQTT client.
                var options = new ManagedMqttClientOptionsBuilder()
                    .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                    .WithClientOptions(new MqttClientOptionsBuilder()
                        .WithClientId("Source")
                        .WithTcpServer("127.0.0.1")
                        .Build())
                    .Build();

                var factory = new MqttFactory();

                var mqttPublisherClient = factory.CreateManagedMqttClient(new MqttNetLogger("MyCustomID"));
                MqttNetGlobalLogger.LogMessagePublished += (s, e) =>
                {
                    var trace = $">> [{e.TraceMessage.Timestamp:O}] [{e.TraceMessage.ThreadId}] [{e.TraceMessage.Source}] [{e.TraceMessage.Level}]: {e.TraceMessage.Message}";
                    if (e.TraceMessage.Exception != null)
                    {
                        trace += Environment.NewLine + e.TraceMessage.Exception.ToString();
                    }

                    Debug.WriteLine('\x2' + trace);
                };

                await mqttPublisherClient.StartAsync(options);
                Console.WriteLine("Source started\n");
                
                while (true)
                {
                    Console.WriteLine("Press 'P' to publish, 'X' to exit.");
                    var c = Console.ReadKey().KeyChar;
                    Console.WriteLine();
                    if (c == 'P' || c == 'p')
                    {
                        for (int i = 0; i < 10; ++i)
                        {
                            // 2018-03-18 KSM moving the message creation into the for-loop eliminates the need to 
                            // have a delay inthe loop
                            var msg = new MqttApplicationMessage
                            {
                                Topic = $"source/property/i{i}",
                                QualityOfServiceLevel = MqttQualityOfServiceLevel.ExactlyOnce,
                                Retain = true,
                                Payload = new byte[] { 30 }
                            };
                            await mqttPublisherClient.PublishAsync(msg);
                            Console.WriteLine($"Published topic: {msg.Topic}");


                            //2018-03-18 KSM delay no longer needed when a new message within the above for-loop
                            //***************************************************************
                            //adjust this value to stop published messages from being dropped
                            //Thread.Sleep(0);
                            //***************************************************************

                        }
                    }
                    else if (c == 'X' || c == 'x')
                    {
                        break;
                    }
                }

                await mqttPublisherClient.StopAsync();
            });

            publisher.Start();
        }
        
    }
}
