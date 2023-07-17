using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Neembly.GPIDServer.WebAPI.Interface;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.WebAPI.Services
{
    public static class AuthenticationBuilderGoogleAdder
    {
        public static AuthenticationBuilder AddGoogleAuth(this AuthenticationBuilder authenticationBuilder, IServiceCollection services)
        {
          var serviceProvider = services.BuildServiceProvider();
            // create IThirdPartyProvidersProvider realization with GetByProviderCode method
            var authThirdPartyProvidersProvider = serviceProvider.GetService<IOperatorSSOQueries>();
            var googleProviders = authThirdPartyProvidersProvider.GetGoogleOperatorSSO();
            if (googleProviders != null)
            {
                googleProviders.ForEach(p =>
                {
                    authenticationBuilder = authenticationBuilder.AddGoogle(options =>
                    {
                        options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                        options.ClientId = p.Client_Id;
                        options.ClientSecret = p.Client_Secret;
                        options.Events.OnRedirectToAuthorizationEndpoint = context =>
                        {
                            context.HttpContext.Response.Redirect(context.RedirectUri + "&prompt=consent"); //also, &prompt=select_account
                            return Task.CompletedTask;
                        };
                    });
                });
            }
            return authenticationBuilder;
        }
    }
}
