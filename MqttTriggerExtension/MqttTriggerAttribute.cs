using System;
using Microsoft.Azure.WebJobs.Description;

namespace MqttTriggerExtension
{
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public sealed class MqttTriggerAttribute : Attribute
    {
        public string Server { get; set; } = "some.server.com";
        public string Topic { get; set; } = "your/topic";
        public int Port { get; set; } = 1883;
        public string ClientId { get; set; } = "MyMqttClient";
    }
}
