using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Noolite2Mqtt.Core.Plugins;
using Noolite2Mqtt.Core.Plugins.Utils;

namespace Noolite2Mqtt.Plugins.Timer
{
    public class TimerPlugin : PluginBase
    {
        #region fields

        public static readonly Random random = new Random();

        public readonly object lockObject = new object();

        public readonly List<InternalTimer> timers = new List<InternalTimer>();

        #endregion

        public override async Task InitPlugin()
        {
            var callbacks = Context.GetAllPlugins()
                .SelectMany(plugin =>
                    plugin.FindMethods<TimerCallbackAttribute, TimerCallbackDelegate>());

            foreach (var callback in callbacks)
            {
                var info = callback.Method.GetMethodInfo();

                Logger.LogInformation($"Register timer callback {info.Name} for {info.DeclaringType.FullName}");

                var timer = new InternalTimer(
                    callback.Meta.Delay ?? random.Next(callback.Meta.Interval),
                    callback.Meta.Interval,
                    callback.Method, Logger);

                timers.Add(timer);
            }
        }

        public override async Task StartPlugin()
        {
            timers.ForEach(timer => timer.Start());
        }

        public override async Task StopPlugin()
        {
            timers.ForEach(timer => timer.Dispose());
        }
    }
}