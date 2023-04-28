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
            var facebookProviders = authThirdPartyProvidersProvider.GetFacebookOperatorSSO();
            if (facebookProviders != null)
            {
                facebookProviders.ForEach(p =>
                {
                    authenticationBuilder = authenticationBuilder.AddFacebook(options =>
                    {
                        options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                        options.ClientId = p.App_Id;
                        options.ClientSecret = p.App_Secret;
                    });
                });
            }
            return authenticationBuilder;
        }
    }
}
