namespace Noolite2Mqtt.Plugins.Mqtt
{
    public delegate void MqttMessageHandlerDelegate(string topic, byte[] payload);
}
