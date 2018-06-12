using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Triggers;
using MQTTnet;

namespace MqttTriggerExtension
{
    public class MqttTriggerAttributeBindingProvider : ITriggerBindingProvider
    {
        private readonly MqttExtentionConfig _extensionConfigProvider;

        public MqttTriggerAttributeBindingProvider(MqttExtentionConfig mqttExtentionConfig)
        {
            _extensionConfigProvider = mqttExtentionConfig;
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var parameter = context.Parameter;
            var attributes = parameter.GetCustomAttributes(false);

            if (attributes == null || attributes.Length == 0)
                return Task.FromResult<ITriggerBinding>(null);

            if (!IsSupportBindingType(parameter.ParameterType))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                    "Can't bind MqttTriggerAttribute to type '{0}'.", parameter.ParameterType));
            }

            return
                Task.FromResult<ITriggerBinding>(new MqttMessageTriggerBinding(context.Parameter,
                    _extensionConfigProvider, context.Parameter.Member.Name));
        }

        public bool IsSupportBindingType(Type t)
        {
            return t == typeof(MqttApplicationMessage) || t == typeof(string);
        }
    }
}
