using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Noolite2Mqtt.Core.Plugins;

namespace Noolite2Mqtt.Core.Infrastructure
{
    public class HomeApplication
    {
        //private IServiceProvider _services;

        private ILogger _logger;

        private IServiceContext _context;
        

        //public async Task StartServices(HomeConfiguration config)
        //{
        //    _services = ConfigureServices(config);

        //    var loggerFactory = _services
        //        .GetRequiredService<ILoggerFactory>();

        //    _logger = loggerFactory.CreateLogger<HomeApplication>();
        //    _context = _services.GetRequiredService<IServiceContext>();

        //    InitLanguage(config);

        //    try
        //    {
        //        // init plugins
        //        foreach (var plugin in _context.GetAllPlugins())
        //        {
        //            _logger.LogInformation($"init plugin: {plugin.GetType().FullName}");
        //            await plugin.InitPlugin();
        //        }

        //        // start plugins
        //        foreach (var plugin in _context.GetAllPlugins())
        //        {
        //            _logger.LogInformation($"start plugin {plugin.GetType().FullName}");
        //            await plugin.StartPlugin();
        //        }

        //        _logger.LogInformation("all plugins are started");
        //    }
        //    catch (ReflectionTypeLoadException ex)
        //    {
        //        _logger.LogError(0, ex, "error on plugins initialization");
        //        foreach (var loaderException in ex.LoaderExceptions)
        //        {
        //            _logger.LogError(0, loaderException, loaderException.Message);
        //        }
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(0, ex, "error on start plugins");
        //        throw;
        //    }
        //}

        public async Task StopServices()
        {
            foreach (var plugin in _context.GetAllPlugins())
            {
                try
                {
                    _logger.LogInformation($"stop plugin {plugin.GetType().FullName}");
                    await plugin.StopPlugin();
                }
                catch (Exception ex)
                {
                    _logger.LogError(0, ex, "error on stop plugins");
                }
            }

            _logger.LogInformation("all plugins are stopped");
        }

        #region private

        private void InitLanguage(HomeConfiguration config)
        {
            var culture = config.GetCulture();
            
            _logger.LogInformation($"init culture: {culture}");
            
            Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture =
                    CultureInfo.DefaultThreadCurrentCulture =
                        CultureInfo.DefaultThreadCurrentUICulture = culture;

        }

        //private static IServiceProvider ConfigureServices(HomeConfiguration config)
        //{
        //    var serviceCollection = new ServiceCollection();

        //    RegisterServices(serviceCollection, config);

        //    return serviceCollection.BuildServiceProvider();
        //}

        public static void RegisterServices(IServiceCollection serviceCollection, HomeConfiguration config)
        {
            var asms = config.GetDependencies().ToArray();
            
            serviceCollection.AddSingleton<IServiceContext, ServiceContext>();
            serviceCollection.AddSingleton(config.Configuration.GetSection("plugins"));

            foreach (var asm in asms)
            {
                AddAssemblyPlugins(serviceCollection, asm);
            }
        }

        private static void AddAssemblyPlugins(IServiceCollection services, Assembly asm)
        {
            var baseType = typeof(PluginBase);

            foreach (var pluginType in asm.GetExportedTypes().Where(type => baseType.GetTypeInfo().IsAssignableFrom(type)))
            {
                services.AddSingleton(baseType, pluginType);
            }
        }

        #endregion
    }
}