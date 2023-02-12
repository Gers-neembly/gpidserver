using Microsoft.Extensions.DependencyInjection;
using Neembly.GPIDServer.SharedServices.Helpers;
using Neembly.GPIDServer.SharedServices.Interfaces;
using Neembly.GPIDServer.SharedServices.SSO;

namespace Neembly.GPIDServer.SharedServices
{
    public static class DependencyInjection
    {
        /// <summary>
        /// DI for shared services
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection Add(this IServiceCollection services)
        {
            //player net
            services.AddScoped<IPlayerNetService, PlayerNetService>();

            //SSO 
            services.AddScoped<ISSOClaimsService, SSOClaimsService>();

            //token provider
            services.AddScoped<ITokenProviderService, TokenProviderService>();

            return services;
        }
    }
}
