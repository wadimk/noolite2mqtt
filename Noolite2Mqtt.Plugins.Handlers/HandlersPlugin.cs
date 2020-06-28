using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Noolite2Mqtt.Core.Plugins;
using Noolite2Mqtt.Core.Plugins.Utils;
using Noolite2Mqtt.Plugins.Mqtt;
using Noolite2Mqtt.Plugins.NooLite;
using Noolite2Mqtt.Plugins.Timer;

namespace Noolite2Mqtt.Plugins.Handlers
{

    public class Payload: IPayload
    {
        public string name { get; set; }
        internal string node_id { get; set; }

        public Payload(string deviceName)
        {
            name = deviceName;
        }

        internal string GetHomeAssistantTopic(string component, string type, string objectId)
        {
            const string discoveryPrefix = "homeassistant";
            const string myPrefix = "noolite2mqtt";

            var prefix = type == "config" ? discoveryPrefix : myPrefix;

            //
            //< component >: One of the supported MQTT components, eg. binary_sensor.
            //    < node_id > (Optional): ID of the node providing the topic, this is not used by Home Assistant but may be used to structure the MQTT topic. The ID of the node must only consist of characters from the character class [a-zA-Z0-9_-]
            //(alphanumerics, underscore and hyphen).
            //    <object_id>: The ID of the device.This is only to allow for separate topics for each device and is not used for the entity_id.The ID of the device must only consist of characters from the character class [a-zA-Z0-9_-]
            //(alphanumerics, underscore and hyphen).


            return $"{prefix}/{component}/{node_id}/{objectId}/{type}";
        }

        public string config_topic { get; set; }

    }

    public interface IPayload
    {
        [JsonIgnore]
        string config_topic { get; set; }
    }

    public class TemperatureSensor : Payload
    {
        public TemperatureSensor(string deviceName) : base(deviceName)
        {
            node_id = "1";
            device_class = "temperature";
            state_topic = GetHomeAssistantTopic("sensor", "state", $"{name}{1}");
            config_topic = GetHomeAssistantTopic("sensor", "config", $"{name}{1}");
            unit_of_measurement = "°C";
            value_template = "{{ value_json.temperature}}";
        }

        public string device_class { get; set; }
        public string state_topic { get; set; }
        public string unit_of_measurement { get; set; }
        public string value_template { get; set; }
    }

    public class Switch : Payload
    {
        public Switch(string deviceName) : base(deviceName)
        {
            node_id = "2";
            state_topic = GetHomeAssistantTopic("switch", "state", $"{name}{1}");
            command_topic = GetHomeAssistantTopic("switch", "set", $"{name}{1}");
            config_topic = GetHomeAssistantTopic("switch", "config", $"{name}{1}");
        }

        public string state_topic { get; set; }
        public string command_topic { get; set; }
    }

    public class HandlersPlugin : PluginBase
    {
        private List<IPayload> hass;

        public override async Task InitPlugin()
        {
            hass = new List<IPayload>();
            hass.Add(new Switch("switch"));
            hass.Add(new TemperatureSensor("temperature"));
        }

        public override async Task StartPlugin()
        {
        }

        public override async Task StopPlugin()
        {

        }

        [NooLiteMicroclimateDataHandler]
        public void MicroclimateDataHandler(int channel, decimal temperature, int? humidity, bool lowBattery)
        {
            Logger.LogInformation($"{temperature}");
            //Context.Require<MqttPlugin>().Publish(GetHomeAssistantTopic("binary_sensor", "state"), "value=12", false);
        }

        [NooLiteCommandHandler]
        public void NooLiteCommandHandler(byte command, int channel, byte format, byte d1, byte d2, byte d3, byte d4)
        {
            Logger.LogInformation($"{command} {channel}");
            Context.Require<MqttPlugin>().Publish("homeassistant/kitchen/temperature", "value=12", false);
        }

        [MqttMessageHandler]
        public void HandleMqttMessage(string topic, byte[] payload)
        {
            var str = Encoding.UTF8.GetString(payload);

            if (topic == "test")
            {
                Logger.LogWarning($"TEST MESSAGE: {str}");
            }
            else
            {
                Logger.LogInformation($"{topic}: {str}");
            }
        }

        [TimerCallback(60000)]
        public void PublishConfiguration(DateTime now)
        {
            foreach (var payload in hass)
            {
                Context.Require<MqttPlugin>().Publish(payload.config_topic, payload.ToJson(), false);
            }
        }

    }
}
