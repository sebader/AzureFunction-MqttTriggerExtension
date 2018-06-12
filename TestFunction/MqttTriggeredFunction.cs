using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using MQTTnet;
using MqttTriggerExtension;
using System.Text;

namespace TestFunction
{
    public class MqttTriggeredFunction
    {     
        [FunctionName("MqttTriggerFunction")]
        public static void Run([MqttTrigger(Server = "my.mqttbroker.com", Topic = "my/topic", ClientId = "MyFunction")]MqttApplicationMessage message, TraceWriter log)
        {
            log.Info($"+ Topic = {message.Topic}");
            log.Info($"+ Payload = {Encoding.UTF8.GetString(message.Payload)}");
            log.Info($"+ QoS = {message.QualityOfServiceLevel}");
            log.Info($"+ Retain = {message.Retain}");
        }

        /*
         * Example below how to bind to message string (expects UTF-8 encoded payload)
        [FunctionName("MqttTriggerFunction")]
        public static void Run([MqttTrigger(Server = "my.mqttbroker.com", Port = 1883, Topic = "my/topic", ClientId = "MyFunction")]string message, TraceWriter log)
        {
            log.Info($"+ Message text = {message}");
        }
        */
    }
}
