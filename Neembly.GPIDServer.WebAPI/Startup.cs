using FluentValidation.AspNetCore;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Neembly.GPIDServer.Persistence;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.Persistence.Helpers;
using Neembly.GPIDServer.Persistence.Interfaces;
using Neembly.GPIDServer.SharedClasses;
using Neembly.GPIDServer.SharedServices.Helpers;
using Neembly.GPIDServer.SharedServices.Interfaces;
using Neembly.GPIDServer.WebAPI.Filters;
using Neembly.GPIDServer.WebAPI.Models.Configs;
using Neembly.GPIDServer.WebAPI.Services;
using Neembly.GPIDServer.WebAPI.Validator;
using System;

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

            var authTokenConfig = new AuthTokenInfo();
            Configuration.Bind("AuthTokenInfo", authTokenConfig);
            services.AddSingleton(authTokenConfig);


            services.AddIdentityServer()
                    .AddDeveloperSigningCredential()
                    .AddInMemoryPersistedGrants()
                    .AddInMemoryIdentityResources(Config.GetIdentityResources())
                    .AddInMemoryApiResources(Config.GetApiResources(authClientConfig.AuthClientResourcesList))
                    .AddInMemoryClients(Config.GetClients(authClientConfig.AuthClientInfoList))
                    .AddAspNetIdentity<AppUser>();

            services.Configure<IdentityOptions>(o => {
                o.SignIn.RequireConfirmedEmail = false;
            });

            var resetPasswordTokenSettings = new ResetPasswordTokenSettings();
            Configuration.GetSection(nameof(ResetPasswordTokenSettings)).Bind(resetPasswordTokenSettings);
            services.AddSingleton(resetPasswordTokenSettings);

            services.Configure<DataProtectionTokenProviderOptions>(opt =>
                opt.TokenLifespan = TimeSpan.FromHours(resetPasswordTokenSettings.TokenLifeSpan));

            // dependency injections
            services.AddScoped<IDataAccess, DataAccess>();
            services.AddScoped<IPlayerNetService, PlayerNetService>();
            services.AddScoped<ITokenProviderService, TokenProviderService>();
            services.AddTransient<IProfileService, IdentityClaimsProfileService>();
            services.AddTransient<IResourceOwnerPasswordValidator, CustomResourceOwnerPasswordValidator>();

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
        }
    }
}
