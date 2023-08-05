using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neembly.GPIDServer.Constants;
using Neembly.GPIDServer.Persistence;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.WebAPI.Filters;
using Neembly.GPIDServer.WebAPI.Interface;
using Neembly.GPIDServer.WebAPI.Models.Configs;
using Neembly.GPIDServer.WebAPI.Queries;
using Neembly.GPIDServer.WebAPI.Services;
using System;
using System.Linq;

namespace Neembly.GPIDServer.WebAPI
{
    public static class DependencyInjectionConfigs
    {
        /// <summary>
        /// DI for configs
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        /// 

        public static IServiceCollection Add(this IServiceCollection services, IConfiguration configuration)
        {
            var paramList = Environment.GetCommandLineArgs();
            int offset = Array.FindIndex(paramList, m => m == "--webname");
            string webname = string.Empty;
            if (offset >= 0) webname = paramList[offset + 1];

            Console.WriteLine($"Loading Webname : {webname}");
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

            //authentication client config
            var authAuthorityHosts = new AuthorityHost();
            configuration.Bind("AuthorityHost", authAuthorityHosts);
            services.AddSingleton(authAuthorityHosts);

            var hostAuthority = string.Empty;

            if (string.IsNullOrEmpty(webname))
                hostAuthority = authAuthorityHosts.AuthorityHostList.Where(p => p.webname == "default").Select(p => p.webaddress).FirstOrDefault();
            else
                hostAuthority = authAuthorityHosts.AuthorityHostList.Where(p => p.webname == webname).Select(p => p.webaddress).FirstOrDefault();

            //add identity server
            services.AddIdentityServer()
                    .AddDeveloperSigningCredential()
                    .AddInMemoryPersistedGrants()
                    .AddInMemoryIdentityResources(Config.GetIdentityResources())
                    .AddInMemoryApiResources(Config.GetApiResources(authClientConfig.AuthClientResourcesList))
                    .AddInMemoryClients(Config.GetClients(authClientConfig.AuthClientInfoList))
                    .AddAspNetIdentity<AppUser>();

            var signInOIDC = authClientConfig.AuthClientInfoList.Where(p => p.Type == GlobalConstants.AuthTypeGrantCode).FirstOrDefault();

            //identity option configs
            services.Configure<IdentityOptions>(o => {
                o.SignIn.RequireConfirmedEmail = false;
            });

            //add cross origin
            services.AddCors();

            // Auth Collection
            services.AddTransient<IOperatorSSOQueries, OperatorSSOQueries>();

            // add authentications
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
            .AddGoogleAuth(services, webname)
            .AddFacebookAuth(services, webname)
            .AddCookie("Cookies")
            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = hostAuthority;
                options.RequireHttpsMetadata = false;       
                options.ClientId = signInOIDC.ClientId;
                options.ClientSecret = signInOIDC.SecretKey;
                options.ResponseType = "code";      
                options.SaveTokens = true;
                options.Scope.Add("openid");
                options.Scope.Add("email");
                options.Scope.Add("profile");
                options.Scope.Add("offline_access");
            });

            //mvc services
            services.AddMvc(options => 
                        options.Filters.Add(typeof(CustomExceptionFilterAttribute)))
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssembly(typeof(Exceptions.ValidationException).Assembly));


            return services;
        }
    }
}
