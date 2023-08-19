using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Neembly.GPIDServer.Security.OAuth.Telegram;
using Neembly.GPIDServer.WebAPI.Interface;
using System;

namespace Neembly.GPIDServer.WebAPI.Services
{
    public static class AuthenticationBuilderTelegram
    {
        public static AuthenticationBuilder AddTelegramAuth(this AuthenticationBuilder authenticationBuilder, IServiceCollection services, string socialAccountName)
        {
            var serviceProvider = services.BuildServiceProvider();
            // create IThirdPartyProvidersProvider realization with GetByProviderCode method
            var authThirdPartyProvidersProvider = serviceProvider.GetService<IOperatorSSOQueries>();
            var telegramProviders = authThirdPartyProvidersProvider.GetTelegramOperatorSSO(socialAccountName);
            if (telegramProviders != null)
            {
                Console.WriteLine($"Loading Telegram Config for Webname : {socialAccountName}");
                authenticationBuilder = authenticationBuilder.AddTelegram(options =>
                {
                      options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                //    options.ClientId = googleProviders.Client_Id;
                //    options.ClientSecret = googleProviders.Client_Secret;
                //    options.Events.OnRedirectToAuthorizationEndpoint = context =>
                //    {
                //        context.Response.Redirect(context.RedirectUri + "&prompt=select_account consent"); //also, &prompt=select_account
                //        return Task.CompletedTask;
                //    };
                });
            }
            else
                Console.WriteLine($"No Telegram Config for Webname : {socialAccountName}");
            return authenticationBuilder;
        }
    }
}
