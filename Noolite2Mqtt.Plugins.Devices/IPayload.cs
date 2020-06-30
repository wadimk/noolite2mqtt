using Newtonsoft.Json;

namespace Noolite2Mqtt.Plugins.Devices
{
    public interface IPayload
    {
        [JsonIgnore]
        string config_topic { get; set; }
        string state_topic { get; set; }

        string Data(object payload);

        string CommandTopic();
    }
}