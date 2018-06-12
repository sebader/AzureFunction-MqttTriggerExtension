using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.Bindings;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using MQTTnet;
using Newtonsoft.Json.Linq;

namespace MqttTriggerExtension
{
    internal class MqttMessageTriggerBinding : ITriggerBinding
    {
        private readonly Dictionary<string, Type> _bindingContract;
        private readonly string _functionName;
        private readonly ParameterInfo _parameter;
        private MqttExtentionConfig _listenersStore;

        public MqttMessageTriggerBinding(ParameterInfo parameter, MqttExtentionConfig listenersStore, string functionName)
        {
            _parameter = parameter;
            _listenersStore = listenersStore;
            _functionName = functionName;
            _bindingContract = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                {"data", typeof(JObject)}
            };
        }

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            if (value is MqttApplicationMessage)
            {
                var bindingData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    {"data", value}
                };

                object argument;
                if (_parameter.ParameterType == typeof(string))
                {
                    // If the binding is string, we assume the payload is a UTF-8 encoded text
                    var message = value as MqttApplicationMessage;
                    if (message?.Payload?.Length > 0)
                    {
                        argument = Encoding.UTF8.GetString(message.Payload);
                    }
                    else
                    {
                        argument = null;
                    }
                }
                else
                {
                    argument = value;
                }

                IValueBinder valueBinder = new MqttValueBinder(_parameter, argument);
                return Task.FromResult<ITriggerData>(new TriggerData(valueBinder, bindingData));
            }
            throw new Exception();
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            var attribute = GetResolvedAttribute<MqttTriggerAttribute>(_parameter);
            return Task.FromResult<IListener>(new MqttListener(context.Executor, attribute));
        }

        /// <summary>Get a description of the binding.</summary>
        /// <returns>The <see cref="T:Microsoft.Azure.WebJobs.Host.Protocols.ParameterDescriptor" /></returns>
        public ParameterDescriptor ToParameterDescriptor()
        {
            return new MqttTriggerParameterDescriptor
            {
                Name = _parameter.Name,
                DisplayHints = new ParameterDisplayHints
                {
                    Prompt = "MqttApplicationMessage",
                    Description = "MqttMessage trigger fired",
                    DefaultValue = "Sample"
                }
            };
        }

        public Type TriggerValueType => typeof(MqttApplicationMessage);

        /// <summary>Gets the binding data contract.</summary>
        public IReadOnlyDictionary<string, Type> BindingDataContract => _bindingContract;

        internal static TAttribute GetResolvedAttribute<TAttribute>(ParameterInfo parameter)
            where TAttribute : Attribute
        {
            var attribute = parameter.GetCustomAttribute<TAttribute>(true);
            return attribute;
        }

        private class MqttValueBinder : ValueBinder, IDisposable
        {
            private readonly object _value;
            private List<IDisposable> _disposables;

            public MqttValueBinder(ParameterInfo parameter, object value,
                List<IDisposable> disposables = null)
                : base(parameter.ParameterType)
            {
                _value = value;
                _disposables = disposables;
            }

            public void Dispose()
            {
                if (_disposables != null)
                {
                    foreach (var d in _disposables)
                        d.Dispose();
                    _disposables = null;
                }
            }

            public override Task<object> GetValueAsync()
            {
                return Task.FromResult(_value);
            }

            public override string ToInvokeString()
            {
                return $"{_value}";
            }
        }

        private class MqttTriggerParameterDescriptor : TriggerParameterDescriptor
        {
            public override string GetTriggerReason(IDictionary<string, string> arguments)
            {
                return $"MQTT trigger fired at {DateTime.Now.ToString("o")}";
            }
        }
    }
}
