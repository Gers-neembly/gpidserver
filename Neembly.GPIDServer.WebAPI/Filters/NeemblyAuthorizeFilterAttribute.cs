using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Neembly.GPIDServer.SharedServices.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.WebAPI.Filters
{
    public class NeemblyAuthorizeAttribute : TypeFilterAttribute
    {

        public NeemblyAuthorizeAttribute()
            : base(typeof(NeemblyAuthorizeFilter))
        {

        }

    }

    public class NeemblyAuthorizeFilter : IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            string url = ($"{context.HttpContext.Request.Scheme.ToString()}://{context.HttpContext.Request.Host.ToString()}");
            string accessToken = context.HttpContext.Request.Headers["Authorization"].ToString().Substring(7);
            var serviceToken = context.HttpContext.RequestServices.GetService<ITokenProviderService>();
            if (!await serviceToken.ValidateToken(accessToken, url))
            {
                context.Result = new UnauthorizedResult();
            }
        }

        
    }
}
