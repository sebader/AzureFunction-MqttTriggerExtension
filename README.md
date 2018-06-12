# Azure Function - Custom Input Binding for MQTT
Custom Input Binding extension for Azure Function v2 to trigger on messages from a MQTT Broker

This extension registers an MQTT client (using MQTTnet library) and fires whenever a new message arrives in the specified topic.
Note that this extension serves mostly as a starting point and should not be considred production-ready code yet! Any pull requets are welcome :)

More information on custom bindings can be found here: https://github.com/Azure/azure-webjobs-sdk/wiki/Creating-custom-input-and-output-bindings

Based on the example from https://github.com/yuka1984/SampleProjects/tree/master/SlackMessageTriggerExtention. Thanks @yuka1984!
