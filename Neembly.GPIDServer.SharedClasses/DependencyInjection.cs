using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Neembly.GPIDServer.SharedClasses
{
    public static class DependencyInjection
    {
        /// <summary>
        /// DI for shared config
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection Add(this IServiceCollection services, IConfiguration configuration)
        {
            //authentication token info config
            var authTokenConfig = new AuthTokenInfo();
            configuration.Bind("AuthTokenInfo", authTokenConfig);
            services.AddSingleton(authTokenConfig);

            //reset pass token settings
            var resetPasswordTokenSettings = new ResetPasswordTokenSettings();
            configuration.GetSection(nameof(ResetPasswordTokenSettings)).Bind(resetPasswordTokenSettings);
            services.AddSingleton(resetPasswordTokenSettings);

            //data protection token provider option configs
            services.Configure<DataProtectionTokenProviderOptions>(opt =>
                opt.TokenLifespan = TimeSpan.FromHours(resetPasswordTokenSettings.TokenLifeSpan));

            return services;
        }
    }
}
