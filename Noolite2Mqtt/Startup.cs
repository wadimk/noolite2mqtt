using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Noolite2Mqtt.Core.Infrastructure;
using Noolite2Mqtt.Core.Plugins;

namespace noolite2mqtt
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddApiVersioning();

            services.Configure<ConsoleLifetimeOptions>(opts => opts.SuppressStatusMessages = true);

            var config = new HomeConfiguration(Configuration);
            HomeApplication.RegisterServices(services, config);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            InitPlugins(app.ApplicationServices, logger);
        }

        public void InitPlugins(IServiceProvider services, ILogger logger)
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
