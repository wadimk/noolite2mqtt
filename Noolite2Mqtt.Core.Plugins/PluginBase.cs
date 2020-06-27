using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Noolite2Mqtt.Core.Plugins
{
    public abstract class PluginBase: IPlugin
    {
        #region properties

        public bool IsInitialized { get; protected set; }

        public IServiceContext Context { get; set; }

        public ILogger Logger { get; set; }

        public IConfigurationSection Configuration { get; set; }

        #endregion

        #region life cycle

        public virtual async Task InitPlugin()
        {
            await Task.CompletedTask;
        }

        public virtual async Task StartPlugin()
        {
            await Task.CompletedTask;
        }

        public virtual async Task StopPlugin()
        {
            await Task.CompletedTask;
        }

        #endregion

        public void SafeInvoke<T>(IEnumerable<T> handlers, Action<T> action, bool async = false)
        {
            if (handlers == null) return;

            foreach (var handler in handlers)
            {
                SafeInvoke(handler, action, async);
            }
        }

        public void SafeInvoke<T>(T handler, Action<T> action, bool async = false)
        {
            if (handler == null) return;

            var context = new EventContext<T>(handler, action, Logger);
            context.Invoke(async);
        }
    }
}
