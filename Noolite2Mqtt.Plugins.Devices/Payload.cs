namespace Noolite2Mqtt.Plugins.Devices
{
    public abstract class Payload : IPayload
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

        public string state_topic { get; set; }

        public abstract string Data(object payload);
        public string CommandTopic()
        {
            if (this is INooliteCommand)
            {
                return ((INooliteCommand) this).command_topic;
            }

            return null;
        }
    }
}