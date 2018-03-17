using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Diagnostics;
using MQTTnet.ManagedClient;
using MQTTnet.Protocol;
using System;
using System.Diagnostics;
using System.Threading;

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

                var msg = new MqttApplicationMessage
                {
                    QualityOfServiceLevel = MqttQualityOfServiceLevel.ExactlyOnce,
                    Retain = true,
                    Payload = new byte[] { 30 }
                };

                while (true)
                {
                    Console.WriteLine("Press 'P' to publish, 'X' to exit.");
                    var c = Console.ReadKey().KeyChar;
                    Console.WriteLine();
                    if (c == 'P' || c == 'p')
                    {
                        for (int i = 0; i < 10; ++i)
                        {
                            msg.Topic = $"source/property/i{i}";
                            await mqttPublisherClient.PublishAsync(msg);
                            Console.WriteLine($"Published topic: {msg.Topic}");

                            //***************************************************************
                            //adjust this value to stop published messages from being dropped
                            Thread.Sleep(0);
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
