using System;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private NooLitePlugin noolite;

        public override async Task InitPlugin()
        {
            mqtt = Context.Require<MqttPlugin>();
            devices = Context.Require<DevicesPlugin>();
            noolite = Context.Require<NooLitePlugin>();
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
            dynamic pl = new ExpandoObject();
            pl.Humidity = humidity;
            pl.Temperature = temperature; 

            foreach (var payload in device.PayloadsList)
            {
                mqtt.TryPublish(payload.config_topic, payload.Data(pl), false);
            }

            
        }

        [NooLiteCommandHandler]
        public void NooLiteCommandHandler(byte command, int channel, byte format, byte d1, byte d2, byte d3, byte d4)
        {
            Logger.LogInformation($"{command} {channel}");
            mqtt.TryPublish("homeassistant/kitchen/temperature", "value=12", false);
        }

        [MqttMessageHandler]
        public void HandleMqttMessage(string topic, byte[] payload)
        {
            var str = Encoding.UTF8.GetString(payload);

           var commandTopics = devices.CommandTopicList;

           var device = devices.GetDevice(topic);

           if (device != null)
           {
               SendCommand(device.Channel, str);
           }
        }


        private string SendCommand(int ch, string command)
        {
            var adapter = noolite.Open(false);

            switch (command.ToLowerInvariant())
            {
                case "on":
                    adapter.On(Convert.ToByte(ch));
                    break;

                case "off":
                    adapter.Off(Convert.ToByte(ch));
                    break;

                case "bind":
                    adapter.Bind(Convert.ToByte(ch));
                    break;

                case "unbind":
                    adapter.UnBind(Convert.ToByte(ch));
                    break;

                default:
                    return "error, command is not supported";
            }


            return $"{ch}: {command}";

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
