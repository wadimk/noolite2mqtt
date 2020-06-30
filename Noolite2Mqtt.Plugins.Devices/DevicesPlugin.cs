using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noolite2Mqtt.Core.Plugins;
namespace Noolite2Mqtt.Plugins.Devices
{
    public class DevicesPlugin : PluginBase
    {
        public IReadOnlyList<NooliteDevice> DevicesList => _devices;
        private List<NooliteDevice> _devices;

        public NooliteDevice GetDevice(int channel, decimal temperature, int? humidity)
        {
            var device = _devices.FirstOrDefault(dev => dev.Channel == channel);

            if (device == null)
            {
                _devices.Add(new NooliteDevice(channel, NooliteDeviceType.TemperatureHumiditySensor));
            }

            return  _devices.First(dev=>dev.Channel == channel);
        }

        public override async Task InitPlugin()
        {

            _devices = new List<NooliteDevice>();
        }

        public override async Task StartPlugin()
        {
            
            _devices.Add(new NooliteDevice(1, NooliteDeviceType.Swith));
            _devices.Add(new NooliteDevice(2, NooliteDeviceType.TemperatureHumiditySensor));
        }

        public override async Task StopPlugin()
        {

        }
    }
}