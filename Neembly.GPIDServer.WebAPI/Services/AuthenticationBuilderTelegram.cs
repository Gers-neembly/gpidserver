using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Neembly.GPIDServer.Security.OAuth.Telegram;
using Neembly.GPIDServer.WebAPI.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                    options.ClientId = telegramProviders.ClientId;
                    options.ClientSecret = telegramProviders.Secret;
                    options.Bot_Id = telegramProviders.Bot_Id;
                    options.Public_Key = telegramProviders.Public_Key;
                    options.Nonce = telegramProviders.Nonce;
                    options.Scope.Add(telegramProviders.Scope);
                    options.Events.OnRedirectToAuthorizationEndpoint = context =>
                    {
                        string urlReferer = context.Request.Headers["Origin"].ToString();
                        context.Response.Redirect(context.RedirectUri + $"&bot_id={telegramProviders.Bot_Id}&public_key={telegramProviders.Public_Key}&nonce={telegramProviders.Nonce}&origin={urlReferer}");
                        return Task.CompletedTask;
                    };
                });
            }
            else
                Console.WriteLine($"No Telegram Config for Webname : {socialAccountName}");
            return authenticationBuilder;
        }
    }
}
