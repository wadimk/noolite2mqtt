using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Noolite2Mqtt.Core.Plugins;
using Noolite2Mqtt.Core.Plugins.Utils;
using Noolite2Mqtt.Plugins.Devices;
using Noolite2Mqtt.Plugins.Mqtt;
using Noolite2Mqtt.Plugins.NooLite;
using Noolite2Mqtt.Plugins.Timer;

namespace Noolite2Mqtt.Plugins.Handlers
{
    public class HandlersPlugin : PluginBase
    {
        private DevicesPlugin devices;
        private MqttPlugin mqtt;

        public override async Task InitPlugin()
        {
            mqtt = Context.Require<MqttPlugin>();
            devices = Context.Require<DevicesPlugin>();
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

            var device = devices.GetDevice(channel, temperature, humidity);

            foreach (var payload in device.PayloadsList)
            {
                mqtt.TryPublish(payload.config_topic, payload.Data(temperature), false);
            }

            
        }

        [NooLiteCommandHandler]
        public void NooLiteCommandHandler(byte command, int channel, byte format, byte d1, byte d2, byte d3, byte d4)
        {
            Logger.LogInformation($"{command} {channel}");
            Context.Require<MqttPlugin>().TryPublish("homeassistant/kitchen/temperature", "value=12", false);
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

            foreach (var payload in devices.DevicesList.SelectMany(dev => dev.PayloadsList))
            {
                Context.Require<MqttPlugin>().TryPublish(payload.config_topic, payload.ToJson(), false);
            }
        }

    }
}
