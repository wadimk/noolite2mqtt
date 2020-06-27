using System.Collections.Generic;

namespace Noolite2Mqtt.Core.Plugins
{
    public interface IServiceContext
    {
        IReadOnlyCollection<PluginBase> GetAllPlugins();

        T Require<T>() where T : PluginBase;
    }
}
