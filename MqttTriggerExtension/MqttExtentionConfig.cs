using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Config;

namespace MqttTriggerExtension
{
    public class MqttExtentionConfig : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            
            context.Config.RegisterBindingExtensions(new MqttTriggerAttributeBindingProvider(this));
        }
    }
}
