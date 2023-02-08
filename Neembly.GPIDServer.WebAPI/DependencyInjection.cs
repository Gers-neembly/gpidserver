using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.Extensions.DependencyInjection;
using Neembly.GPIDServer.WebAPI.Interface;
using Neembly.GPIDServer.WebAPI.Queries;
using Neembly.GPIDServer.WebAPI.Services;
using Neembly.GPIDServer.WebAPI.Validator;

namespace Neembly.GPIDServer.WebAPI
{
    public static class DependencyInjection
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection Add(this IServiceCollection services)
        {
            //identity claims profile
            services.AddTransient<IProfileService, IdentityClaimsProfileService>();
            
            //resource owner password validator
            services.AddTransient<IResourceOwnerPasswordValidator, CustomResourceOwnerPasswordValidator>();

            // Auth Collection
            services.AddTransient<IOperatorSSOQueries, OperatorSSOQueries>();

            return services;
        }
    }
}
