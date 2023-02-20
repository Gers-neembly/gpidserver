using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Neembly.GPIDServer.WebAPI.Interface;


namespace Neembly.GPIDServer.WebAPI.Services
{
    public static class AuthenticationBuilderFacebook
    {
        public static AuthenticationBuilder AddFacebookAuth(this AuthenticationBuilder authenticationBuilder, IServiceCollection services)
        {
          var serviceProvider = services.BuildServiceProvider();
            // create IThirdPartyProvidersProvider realization with GetByProviderCode method
            var authThirdPartyProvidersProvider = serviceProvider.GetService<IOperatorSSOQueries>();
            var googleProviders = authThirdPartyProvidersProvider.GetperatorSSOByProvider("facebook");
            if (googleProviders != null)
            {
                googleProviders.ForEach(p =>
                {
                    authenticationBuilder = authenticationBuilder.AddFacebook(options =>
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
