using Newtonsoft.Json;

namespace Noolite2Mqtt.Plugins.Devices
{
    public class TemperatureSensor : Payload
    {
        public TemperatureSensor(string deviceName) : base(deviceName)
        {
            node_id = "1";
            device_class = "temperature";
            state_topic = GetHomeAssistantTopic("sensor", "state", $"{name}");
            config_topic = GetHomeAssistantTopic("sensor", "config", $"{name}");
            unit_of_measurement = "°C";
            value_template = "{{ value_json.temperature}}";
        }

        public string device_class { get; set; }
        public override string Data(object payload)
        {
            return JsonConvert.SerializeObject(payload);
        }

        public string unit_of_measurement { get; set; }
        public string value_template { get; set; }
    }
}