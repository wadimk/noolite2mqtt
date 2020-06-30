namespace Noolite2Mqtt.Plugins.Devices
{
    public class Switch : Payload, INooliteCommand
    {
        public Switch(string deviceName) : base(deviceName)
        {
            node_id = "2";
            state_topic = GetHomeAssistantTopic("switch", "state", $"{name}");
            command_topic = GetHomeAssistantTopic("switch", "set", $"{name}");
            config_topic = GetHomeAssistantTopic("switch", "config", $"{name}");
        }

        
        public override string Data(object payload)
        {
            return "";
        }

        public string command_topic { get; set; }
    }

    public interface INooliteCommand
    {
        string command_topic { get; set; }
}
}