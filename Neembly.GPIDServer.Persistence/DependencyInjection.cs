using Microsoft.Extensions.DependencyInjection;
using Neembly.GPIDServer.Persistence.Helpers;
using Neembly.GPIDServer.Persistence.Interfaces;

namespace Neembly.GPIDServer.Persistence
{
    public static class DependencyInjection
    {
        /// <summary>
        /// DI for persistence
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection Add(this IServiceCollection services)
        {
            //data access
            services.AddScoped<IDataAccess, DataAccess>();

            return services;
        }
    }
}
