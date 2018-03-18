using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Diagnostics;
using MQTTnet.ManagedClient;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace MQTTSubscriberTest
{
    class Subscriber
    {
        static void Main()
        {
            Thread subscriber = new Thread(async () =>
            {
                // Setup and start a managed MQTT client.
                var options = new ManagedMqttClientOptionsBuilder()
                    .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                    .WithClientOptions(new MqttClientOptionsBuilder()
                        .WithClientId("Sink")
                        .WithTcpServer("127.0.0.1")
                        .Build())
                    .Build();

                var factory = new MqttFactory();

                var mqttSubscriberClient = factory.CreateManagedMqttClient(new MqttNetLogger("MyCustomID"));
                MqttNetGlobalLogger.LogMessagePublished += (s, e) =>
                {
                    var trace =
                        $">> [{e.TraceMessage.Timestamp:O}] [{e.TraceMessage.ThreadId}] [{e.TraceMessage.Source}] [{e.TraceMessage.Level}]: {e.TraceMessage.Message}";
                    if (e.TraceMessage.Exception != null)
                    {
                        trace += Environment.NewLine + e.TraceMessage.Exception.ToString();
                    }

                    Debug.WriteLine('\x2' + trace);
                };

                mqttSubscriberClient.ApplicationMessageReceived += (s, e) =>
                {
                    //Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                    Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                    //Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                    //Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                    //Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                    //Console.WriteLine();
                };

                await mqttSubscriberClient.StartAsync(options);
                Console.WriteLine("Sink client started");

                for (int i = 0; i < 10; ++i)
                {
                    var topic = $"source/property/i{i}";
                    await mqttSubscriberClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(topic).WithExactlyOnceQoS()
                        .Build());
                    Console.WriteLine($"Subscribed to {topic}");
                }

                Console.WriteLine("\nSubscriptions complete.\n\n");
                Console.ReadLine();
            });

            subscriber.Start();
        }
    }
}
