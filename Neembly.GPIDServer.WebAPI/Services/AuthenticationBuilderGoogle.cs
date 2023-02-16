using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Neembly.GPIDServer.WebAPI.Interface;

namespace Neembly.GPIDServer.WebAPI.Services
{
    public static class AuthenticationBuilderGoogleAdder
    {
        public static AuthenticationBuilder AddGoogleAuth(this AuthenticationBuilder authenticationBuilder, IServiceCollection services)
        {
          var serviceProvider = services.BuildServiceProvider();
            // create IThirdPartyProvidersProvider realization with GetByProviderCode method
            var authThirdPartyProvidersProvider = serviceProvider.GetService<IOperatorSSOQueries>();
            var googleProviders = authThirdPartyProvidersProvider.GetperatorSSOByProvider("google");
            if (googleProviders != null)
            {
                googleProviders.ForEach(p =>
                {
                    authenticationBuilder = authenticationBuilder.AddGoogle(options =>
                    {
                        options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                        options.ClientId = p.Client_Id;
                        options.ClientSecret = p.Client_Secret;
                    });
                });
            }
            return authenticationBuilder;
        }
    }
}
