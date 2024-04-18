using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Neembly.GPIDServer.WebAPI.Interface;
using System;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.WebAPI.Services
{
    public static class AuthenticationBuilderGoogleAdder
    {
        public static AuthenticationBuilder AddGoogleAuth(this AuthenticationBuilder authenticationBuilder, IServiceCollection services, string socialAccountName)
        {
          var serviceProvider = services.BuildServiceProvider();
            // create IThirdPartyProvidersProvider realization with GetByProviderCode method
            var authThirdPartyProvidersProvider = serviceProvider.GetService<IOperatorSSOQueries>();
            var googleProviders = authThirdPartyProvidersProvider.GetGoogleOperatorSSO(socialAccountName);
            if (googleProviders != null)
            {
                Console.WriteLine($"Loading Google Config for Webname : {socialAccountName}");
                authenticationBuilder = authenticationBuilder.AddGoogle(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = googleProviders.Client_Id;
                    options.ClientSecret = googleProviders.Client_Secret;
                    options.Events.OnRemoteFailure = context =>
                    {
                        context.Response.Redirect("Auth/AccessDenied?oAuth=Google");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    };
                    options.Events.OnRedirectToAuthorizationEndpoint = context =>
                    {
                        context.Response.Redirect(context.RedirectUri + "&prompt=select_account consent"); //also, &prompt=select_account
                        return Task.CompletedTask;
                    };
                });
            }
            else
                Console.WriteLine($"No Google Config for Webname : {socialAccountName}");
            return authenticationBuilder;
        }
    }
}
