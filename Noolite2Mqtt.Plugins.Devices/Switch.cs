namespace Noolite2Mqtt.Plugins.Devices
{
    public class Switch : Payload
    {
        public Switch(string deviceName) : base(deviceName)
        {
            node_id = "2";
            state_topic = GetHomeAssistantTopic("switch", "state", $"{name}{1}");
            command_topic = GetHomeAssistantTopic("switch", "set", $"{name}{1}");
            config_topic = GetHomeAssistantTopic("switch", "config", $"{name}{1}");
        }

        
        public override string Data(object payload)
        {
            return "";
        }

        public string command_topic { get; set; }
    }
}