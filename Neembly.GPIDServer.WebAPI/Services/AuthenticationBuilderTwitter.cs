using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Neembly.GPIDServer.WebAPI.Interface;
using System;

namespace Neembly.GPIDServer.WebAPI.Services
{
    public static class AuthenticationBuilderTwitterAdder
    {
        public static AuthenticationBuilder AddTwitterAuth(this AuthenticationBuilder authenticationBuilder, IServiceCollection services, string socialAccountName)
        {
            var serviceProvider = services.BuildServiceProvider();
            var authThirdPartyProvidersProvider = serviceProvider.GetService<IOperatorSSOQueries>();
            var twitterProviders = authThirdPartyProvidersProvider.GetTwitterOperatorSSO(socialAccountName);
            if (twitterProviders != null)
            {
                Console.WriteLine($"Loading Twitter Config for Webname : {socialAccountName}");
                authenticationBuilder = authenticationBuilder.AddTwitter(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ConsumerKey = twitterProviders.Api_Key;
                    options.ConsumerSecret = twitterProviders.Api_Secret;
                });
            }
            else
                Console.WriteLine($"No Twitter Config for Webname : {socialAccountName}");
            return authenticationBuilder;
        }
    }
}
