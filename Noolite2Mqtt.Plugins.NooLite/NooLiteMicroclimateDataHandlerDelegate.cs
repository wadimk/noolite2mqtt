﻿namespace Noolite2Mqtt.Plugins.NooLite
{
    public delegate void NooLiteMicroclimateDataHandlerDelegate(int channel, decimal temperature, int? humidity, bool lowBattery);
}
