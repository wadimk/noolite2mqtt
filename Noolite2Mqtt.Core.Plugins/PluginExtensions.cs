using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Noolite2Mqtt.Core.Plugins
{
    public static class PluginExtensions
    {
        public static IApplicationBuilder UsePlugins(
            this IApplicationBuilder builder, ILogger logger)

        {
            InitPlugins(builder.ApplicationServices, logger);

            return builder;
        }

        private static void InitPlugins(IServiceProvider services, ILogger logger)
        {
            logger.LogInformation($"init plugins");

            var context = services.GetRequiredService<IServiceContext>();

            try
            {
                // init plugins
                var iniTasks = new List<Task>();
                foreach (var plugin in context.GetAllPlugins())
                {
                    logger.LogInformation($"init plugin: {plugin.GetType().FullName}");

                    try
                    {
                        iniTasks.Add(plugin.InitPlugin()); ;
                    }
                    catch (NotImplementedException ex)
                    {
                        logger.LogInformation(0, ex, $"{plugin.GetType().FullName} is not initialized");
                    }
                }
                Task.WaitAll(iniTasks.ToArray());

                // start plugins
                var startTasks = new List<Task>();
                foreach (var plugin in context.GetAllPlugins())
                {
                    logger.LogInformation($"start plugin {plugin.GetType().FullName}");

                    try
                    {
                        startTasks.Add(plugin.StartPlugin());
                    }
                    catch (NotImplementedException ex)
                    {
                        logger.LogInformation(0, ex, $"{plugin.GetType().FullName} is not started");
                    }
                }
                Task.WaitAll(startTasks.ToArray());

                logger.LogInformation("all plugins are started");
            }
            catch (ReflectionTypeLoadException ex)
            {
                logger.LogError(0, ex, "error on plugins initialization");
                foreach (var loaderException in ex.LoaderExceptions)
                {
                    logger.LogError(0, loaderException, loaderException.Message);
                }

                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(0, ex, "error on start plugins");
                throw;
            }
        }

    }
}