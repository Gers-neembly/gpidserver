using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Neembly.GPIDServer.WebAPI
{
    public class Startup
    {
        private readonly ILogger _logger;
        public Startup(IConfiguration configuration, ILoggerFactory logFactory)
        {
            _logger = logFactory.CreateLogger<Startup>();
            _logger.LogInformation($"{Process.GetCurrentProcess()} booting...");
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //DI for services
            DependencyInjection.Add(services);

            //DI for configs
            DependencyInjectionConfigs.Add(services, Configuration);

            //DI for shared classes
            SharedClasses.DependencyInjection.Add(services, Configuration);

            //DI for shared services
            SharedServices.DependencyInjection.Add(services);

            //DI for persistence
            Persistence.DependencyInjection.Add(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            //// global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());


            var forwardOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                RequireHeaderSymmetry = false
            };

            forwardOptions.KnownNetworks.Clear();
            forwardOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(forwardOptions);
            app.UseIdentityServer();
            app.UseHttpsRedirection();
            app.UseMvc();

            _logger.LogInformation($"{Process.GetCurrentProcess().MainModule.FileName} started {DateTime.Now}");
        }
    }
}
