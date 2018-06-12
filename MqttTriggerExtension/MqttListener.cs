using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using MQTTnet;
using MQTTnet.Client;

namespace MqttTriggerExtension
{
    public class MqttListener : IListener
    {
        private MqttFactory _factory;
        private IMqttClient _client;
        private MqttClientOptions _mqttOptions;

        private MqttTriggerAttribute _attribute;

        public MqttListener(ITriggeredFunctionExecutor executor, MqttTriggerAttribute attribute)
        {
            Executor = executor;
            _attribute = attribute;

            _mqttOptions = new MqttClientOptions
            {
                ClientId = _attribute.ClientId,
                CleanSession = true,
                ChannelOptions = new MqttClientTcpOptions
                {
                    Server = _attribute.Server,
                    Port = _attribute.Port
                }
            };
        }

        public ITriggeredFunctionExecutor Executor { get; }
        public void Cancel() { }
        public void Dispose() { }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _factory = new MqttFactory();
            _client = _factory.CreateMqttClient();

            _client.ApplicationMessageReceived += OnMessageReceived;
            _client.Connected += OnConnected;
            _client.Disconnected += OnDisconnected;

            try
            {
                await _client.ConnectAsync(_mqttOptions);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"### CONNECTING FAILED TO SERVER {_attribute.Server} ###" + Environment.NewLine + exception);
            }
        }

        private async void OnConnected(object sender, MqttClientConnectedEventArgs e)
        {
            Console.WriteLine($"### MQTT CLIENT CONNECTED TO SERVER {_attribute.Server} ###");
            Console.WriteLine($"### SUBSCRIBING TO TOPIC {_attribute.Topic} ###");
            await _client.SubscribeAsync(new TopicFilterBuilder().WithTopic(_attribute.Topic).Build());
        }

        private async void OnDisconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            Console.WriteLine("### DISCONNECTED FROM SERVER ###");
            // Wait 5 seconds and try to reconnect
            await Task.Delay(TimeSpan.FromSeconds(5));

            try
            {
                await _client.ConnectAsync(_mqttOptions);
            }
            catch
            {
                Console.WriteLine("### RECONNECTING FAILED ###");
            }
        }

        private async void OnMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            var triggerData = new TriggeredFunctionData
            {
                TriggerValue = e.ApplicationMessage
            };

            await Executor.TryExecuteAsync(triggerData, CancellationToken.None);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_client != null)
                return _client.DisconnectAsync();
            return null;
        }
    }
}
