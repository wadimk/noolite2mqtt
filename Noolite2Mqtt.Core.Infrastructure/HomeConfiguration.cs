using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Noolite2Mqtt.Core.Infrastructure
{
    public class HomeConfiguration
    {
        public readonly IConfiguration Configuration;
        

        public HomeConfiguration(IConfiguration config)
        {
            Configuration = config;
        }

        public IConfigurationSection GetPluginSection(Type type)
        {
            return Configuration.GetSection($"plugins:{type.FullName}");
        }

        public IEnumerable<Assembly> GetDependencies()
        {
            return Configuration.GetSection("assemblies")
                .GetChildren()
                .Select(asm =>
                {
                    var name = new AssemblyName(asm.Value);
                    return Assembly.Load(name);
                });
        }

        public CultureInfo GetCulture() 
        {
            var cultureName = Configuration["culture"] ?? string.Empty;
            return CultureInfo.GetCultureInfo(cultureName);
        }
    }
}
