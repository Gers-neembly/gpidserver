using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Neembly.GPIDServer.Security.OAuth.Telegram
{
    public static class TelegramAuthenticationExtensions
    {
        public static AuthenticationBuilder AddTelegram(this AuthenticationBuilder builder)
        {
            return builder.AddTelegram(TelegramAuthenticationDefaults.AuthenticationScheme, options => { });
        }

        public static AuthenticationBuilder AddTelegram(
            this AuthenticationBuilder builder,
            Action<TelegramAuthenticationOptions> configuration)
        {
            return builder.AddTelegram(TelegramAuthenticationDefaults.AuthenticationScheme, configuration);
        }

        public static AuthenticationBuilder AddTelegram(
            this AuthenticationBuilder builder,
            string scheme,
            Action<TelegramAuthenticationOptions> configuration)
        {
            return builder.AddTelegram(scheme, TelegramAuthenticationDefaults.DisplayName, configuration);
        }

        public static AuthenticationBuilder AddTelegram(
            this AuthenticationBuilder builder,
            string scheme,
            string caption,
            Action<TelegramAuthenticationOptions> configuration)
        {
            return builder.AddOAuth<TelegramAuthenticationOptions, TelegramAuthenticationHandler>(scheme, caption, configuration);
        }
    }
}
