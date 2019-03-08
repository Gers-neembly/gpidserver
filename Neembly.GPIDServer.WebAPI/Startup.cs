using FluentValidation.AspNetCore;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neembly.GPIDServer.Persistence;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.Persistence.Helpers;
using Neembly.GPIDServer.Persistence.Interfaces;
using Neembly.GPIDServer.SharedServices.Helpers;
using Neembly.GPIDServer.SharedServices.Interfaces;
using Neembly.GPIDServer.WebAPI.Filters;
using Neembly.GPIDServer.WebAPI.Models.Configs;
using Neembly.GPIDServer.WebAPI.Services;

namespace Neembly.GPIDServer.WebAPI
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
            // Add DBContext services
            services.AddDbContext<AppDBContext>(options =>
                                               options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDbContext<UtilsDBContext>(options =>
                                               options.UseNpgsql(Configuration.GetConnectionString("UtilsConnection")));
            //// Add Identity services
            services.AddIdentity<AppUser, IdentityRole>(user =>
                    {
                        // configure identity options
                        user.Password.RequireDigit = false;
                        user.Password.RequireLowercase = false;
                        user.Password.RequireUppercase = false;
                        user.Password.RequireNonAlphanumeric = false;
                        user.Password.RequiredLength = 2;
                    })
                    .AddEntityFrameworkStores<AppDBContext>()
                    .AddDefaultTokenProviders();

            var authClientConfig = new AuthClientConfiguration();
            Configuration.Bind("AuthClientConfiguration", authClientConfig);
            services.AddSingleton(authClientConfig);

            services.AddIdentityServer()
                    .AddDeveloperSigningCredential()
                    .AddInMemoryPersistedGrants()
                    .AddInMemoryIdentityResources(Config.GetIdentityResources())
                    .AddInMemoryApiResources(Config.GetApiResources(authClientConfig.AuthClientResourcesList))
                    .AddInMemoryClients(Config.GetClients(authClientConfig.AuthClientInfoList))
                    .AddAspNetIdentity<AppUser>();

            services.Configure<IdentityOptions>(o => {
                o.SignIn.RequireConfirmedEmail = true;
            });

            // dependency injections
            services.AddScoped<IDataAccess, DataAccess>();
            services.AddScoped<IEmailDispatcher, EmailDispatcher>();
            services.AddScoped<IEmailQueueService, DbEmailQueueService>();
            services.AddScoped<IExtensionProviders, ExtensionProviders>();
            services.AddTransient<IProfileService, IdentityClaimsProfileService>();

            services.AddCors();
            services
                .AddMvc(options => options.Filters.Add(typeof(CustomExceptionFilterAttribute)))
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(typeof(Exceptions.ValidationException).Assembly));
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

            app.UseIdentityServer();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
