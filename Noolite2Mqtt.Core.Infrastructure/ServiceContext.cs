using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Noolite2Mqtt.Core.Plugins;

namespace Noolite2Mqtt.Core.Infrastructure
{
    public class ServiceContext : IServiceContext
    {
        private readonly Dictionary<Type, PluginBase> plugins;

        public ServiceContext(
            IEnumerable<PluginBase> loadedPlugins,
            IConfigurationSection configuration,
            ILogger<PluginBase> logger)
        {
            plugins = loadedPlugins.ToDictionary(p => p.GetType());

            
            foreach (var plugin in plugins.Values)
            {
                var type = plugin.GetType();

                plugin.Context = this;
                plugin.Logger = logger;
                plugin.Configuration = configuration.GetSection(type.FullName);
            }
        }

        public IReadOnlyCollection<PluginBase> GetAllPlugins()
        {
            return new ReadOnlyCollection<PluginBase>(plugins.Values.ToList());
        }


        public T Require<T>() where T : PluginBase
        {
            return plugins[typeof(T)] as T;
        }
    }
}