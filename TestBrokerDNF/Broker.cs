using System;
using System.Diagnostics;
using System.Threading;
using MQTTnet;
using MQTTnet.Diagnostics;
using MQTTnet.Server;
using Exception = System.Exception;

namespace MQTTBrokerTest
{
    class Boker
    {
        private static IMqttServer mqttBroker;

        static void Main()
        {
            Thread broker = new Thread(async () =>
            {
                MqttNetGlobalLogger.LogMessagePublished += (s, e) =>
                    {
                        var trace =
                            $">> [{e.TraceMessage.Timestamp:O}] [{e.TraceMessage.ThreadId}] [{e.TraceMessage.Source}] [{e.TraceMessage.Level}]: {e.TraceMessage.Message}";
                        if (e.TraceMessage.Exception != null)
                        {
                            trace += Environment.NewLine + e.TraceMessage.Exception.ToString();
                        }
                        Debug.WriteLine('\x1' + trace);
                    };

                mqttBroker = new MqttFactory().CreateMqttServer();
                await mqttBroker.StartAsync(new MqttServerOptions());

            });

            broker.Start();
            Console.WriteLine("Broker started");
            Console.ReadLine();
        }
    }
}
