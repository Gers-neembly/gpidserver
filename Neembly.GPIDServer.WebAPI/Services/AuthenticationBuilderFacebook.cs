using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Neembly.GPIDServer.WebAPI.Interface;
using System;

namespace Neembly.GPIDServer.WebAPI.Services
{
    public static class AuthenticationBuilderFacebook
    {
        public static AuthenticationBuilder AddFacebookAuth(this AuthenticationBuilder authenticationBuilder, IServiceCollection services, string socialAccountName = "betenjoy")
        {
          var serviceProvider = services.BuildServiceProvider();
            // create IThirdPartyProvidersProvider realization with GetByProviderCode method
            var authThirdPartyProvidersProvider = serviceProvider.GetService<IOperatorSSOQueries>();
            var facebookProviders = authThirdPartyProvidersProvider.GetFacebookOperatorSSO(socialAccountName);
            if (facebookProviders != null)
            {
                Console.WriteLine($"Loading Facebook Config for Webname : {socialAccountName}");
                authenticationBuilder = authenticationBuilder.AddFacebook(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = facebookProviders.App_Id;
                    options.ClientSecret = facebookProviders.App_Secret;
                });
            }
            else
                Console.WriteLine($"No Facebook Config for Webname : {socialAccountName}");
            return authenticationBuilder;
        }
    }
}
