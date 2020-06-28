using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Noolite2Mqtt.Core.Infrastructure
{
    public static class ServiceProviderExtensions
    {
        public static void AddPluginsService(this IServiceCollection services, IConfiguration configuration)
        {
            var config = new HomeConfiguration(configuration);
            HomeApplication.RegisterServices(services, config);
        }
    }
}
