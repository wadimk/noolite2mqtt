using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Noolite2Mqtt.Core.Plugins
{
    public interface IPlugin
    {
        Task InitPlugin();
        Task StartPlugin();
        Task StopPlugin();
        
        IConfigurationSection Configuration { get; set; }
        ILogger Logger { get; set; }

        void SafeInvoke<T>(IEnumerable<T> handlers, Action<T> action, bool async = false);
        void SafeInvoke<T>(T handler, Action<T> action, bool async = false);
    }
}