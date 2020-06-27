﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Noolite2Mqtt.Core.Plugins;
using Noolite2Mqtt.Core.Plugins.Utils;
using ThinkingHome.NooLite;

namespace Noolite2Mqtt.Plugins.NooLite
{
    using CommandAttribute = NooLiteCommandHandlerAttribute;
    using CommandDelegate = NooLiteCommandHandlerDelegate;
    using MicroclimateAttribute = NooLiteMicroclimateDataHandlerAttribute;
    using MicroclimateDelegate = NooLiteMicroclimateDataHandlerDelegate;

    public class NooLitePlugin : PluginBase
    {
        private MTRFXXAdapter device;
        private AdapterWrapper wrapper;
        private AdapterWrapper wrapperF;

        private List<CommandDelegate> cmdHandlers = new List<CommandDelegate>();
        private List<MicroclimateDelegate> microclimateHandlers = new List<MicroclimateDelegate>();

        public override async Task InitPlugin()
        {
            var portName = Configuration["portName"];

            if (string.IsNullOrEmpty(portName)) throw new Exception("noolite portName is required");

            Logger.LogInformation($"Use '{portName}' serial port");

            device = new MTRFXXAdapter(portName);
            device.Connect += OnConnect;
            device.Disconnect += OnDisconnect;
            device.ReceiveData += OnReceiveData;
            device.ReceiveMicroclimateData += OnReceiveMicroclimateData;
            device.Error += OnError;


            wrapper = new AdapterWrapper(false, device, Logger);
            wrapperF = new AdapterWrapper(true, device, Logger);

            #region register handlers

            foreach (var plugin in Context.GetAllPlugins())
            {
                var pluginType = plugin.GetType();

                foreach (var mi in plugin
                    .FindMethods<CommandAttribute, CommandDelegate>())
                {
                    Logger.LogInformation(
                        $"register noolite command handler: \"{mi.Method.Method.Name}\" ({pluginType.FullName})");
                    cmdHandlers.Add(mi.Method);
                }

                foreach (var mi in plugin
                    .FindMethods<MicroclimateAttribute, MicroclimateDelegate>())
                {
                    Logger.LogInformation(
                        $"register noolite microclimate handler: \"{mi.Method.Method.Name}\" ({pluginType.FullName})");
                    microclimateHandlers.Add(mi.Method);
                }
            }

            #endregion
        }

        #region events

        private void OnError(object obj, Exception ex)
        {
            Logger.LogError(ex, "MTRF adapter error");
        }

        private void OnConnect(object obj)
        {
            Logger.LogInformation("MTRF adapter connected");
        }

        private void OnDisconnect(object obj)
        {
            Logger.LogInformation("MTRF adapter disconnected");
        }

        private void OnReceiveData(object obj, ReceivedData cmd)
        {
            SafeInvoke(cmdHandlers, h => h((byte)cmd.Command, cmd.Channel, cmd.DataFormat,
                cmd.Data1, cmd.Data2, cmd.Data3, cmd.Data4), true);

        }

        private void OnReceiveMicroclimateData(object obj, MicroclimateData data)
        {
            SafeInvoke(microclimateHandlers, h => h(data.Channel, data.Temperature, data.Humidity, data.LowBattery), true);
        }

        #endregion

        public override async Task StartPlugin()
        {
            device.Open();
        }

        public override async Task StopPlugin()
        {
            device.Dispose();
        }

        //[TimerCallback(20000)]
        public void Reconnect(DateTime now)
        {
            device.Open();
        }

        public AdapterWrapper Open(bool fMode)
        {
            return fMode ? wrapperF : wrapper;
        }
    }
}
