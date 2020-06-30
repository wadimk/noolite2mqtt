using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Noolite2Mqtt.Plugins.Devices;

namespace Noolite2Mqtt.Test
{
    [TestClass]
    public class UnitTest1
    {
        private List<NooliteDevice> _devices;
        public UnitTest1()
        {
            _devices = new List<NooliteDevice>();
            _devices.Add(new NooliteDevice(1, NooliteDeviceType.Swith));
            _devices.Add(new NooliteDevice(2, NooliteDeviceType.TemperatureHumiditySensor));
        }

        [TestMethod]
        public void TestMethod1()
        {
            var payloads = _devices.First(d => d.Channel == 2).PayloadsList;

            dynamic pl = new ExpandoObject();
            pl.Humidity = 90;
            pl.Temperature = 25.2;


            foreach (var payload in payloads)
            {
                var  a = payload.Data(pl);
            }
        }
    }
}
