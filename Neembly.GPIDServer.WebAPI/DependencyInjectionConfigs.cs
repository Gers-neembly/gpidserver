using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neembly.GPIDServer.Persistence;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.WebAPI.Filters;
using Neembly.GPIDServer.WebAPI.Interface;
using Neembly.GPIDServer.WebAPI.Models.Configs;
using Neembly.GPIDServer.WebAPI.Queries;
using Neembly.GPIDServer.WebAPI.Services;

namespace Neembly.GPIDServer.WebAPI
{
    public static class DependencyInjectionConfigs
    {
        /// <summary>
        /// DI for configs
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection Add(this IServiceCollection services, IConfiguration configuration)
        {
            //application database context
            services.AddDbContext<AppDBContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            //add identity config
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

            services.ConfigureApplicationCookie(config =>
            {
                config.Cookie.Name = "Winka.Identity.Cookie";
                config.LoginPath = "/Auth/Login";
            });

            //authentication client config
            var authClientConfig = new AuthClientConfiguration();
            configuration.Bind("AuthClientConfiguration", authClientConfig);
            services.AddSingleton(authClientConfig);

            //add identity server
            services.AddIdentityServer()
                    .AddDeveloperSigningCredential()
                    .AddInMemoryPersistedGrants()
                    .AddInMemoryIdentityResources(Config.GetIdentityResources())
                    .AddInMemoryApiResources(Config.GetApiResources(authClientConfig.AuthClientResourcesList))
                    .AddInMemoryClients(Config.GetClients(authClientConfig.AuthClientInfoList))
                    .AddAspNetIdentity<AppUser>();

            //identity option configs
            services.Configure<IdentityOptions>(o => {
                o.SignIn.RequireConfirmedEmail = false;
            });

            //add cross origin
            services.AddCors();

            // Auth Collection
            services.AddTransient<IOperatorSSOQueries, OperatorSSOQueries>();

            // add authentications
            services.AddAuthentication()
                    .AddGoogleAuth(services)
                    .AddFacebookAuth(services);

            //mvc services
            services.AddMvc(options => 
                        options.Filters.Add(typeof(CustomExceptionFilterAttribute)))
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(typeof(Exceptions.ValidationException).Assembly));


            return services;
        }
    }
}
