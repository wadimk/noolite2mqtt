using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Noolite2Mqtt.Plugins.Devices
{
    public enum NooliteDeviceType
    {
        Swith,
        TemperatureHumiditySensor

    }

    public class NooliteDevice
    {
        private readonly int channel;
        private string name;
        private readonly List<IPayload> _payloadList = new List<IPayload>();

        public int Channel => channel;

        public string StateTopic => _payloadList.First().state_topic;
        public IReadOnlyList<IPayload> PayloadsList => _payloadList;

        public NooliteDevice(int channel, NooliteDeviceType deviceType)
        {
            this.channel = channel;

            switch (deviceType)
            {
                case NooliteDeviceType.Swith:
                    _payloadList.Add(new Switch("switch"));
                    return;

                case NooliteDeviceType.TemperatureHumiditySensor:
                    //_payloadList.Add(new TemperatureSensor("temperature"));
                    _payloadList.Add(new TemperatureSensor("temperature"));
                    return;
            }
        }

        public NooliteDevice(int channel)
        {
            this.channel = channel;
        }
    }
}
