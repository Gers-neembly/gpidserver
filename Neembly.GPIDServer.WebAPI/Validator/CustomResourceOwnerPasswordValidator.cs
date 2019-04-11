using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using Neembly.GPIDServer.Persistence;
using Neembly.GPIDServer.Persistence.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.WebAPI.Validator
{
    public class CustomResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        #region Member Variable
        private readonly AppDBContext _context;
        private readonly UserManager<AppUser> _userManager;
        #endregion

        public CustomResourceOwnerPasswordValidator(
            AppDBContext context,
             UserManager<AppUser> userManager
            )
        {
            _context = context;
            _userManager = userManager;
        }

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            string operatorId = context.Request.Raw["operatorId"];
            string email = context.Request.Raw["email"];
            string userName = $"{context.UserName}_{operatorId}";
            var user = _context.Users.Where(p => p.UserName == userName
                                            && p.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (user != null)
            {
                bool passwordOk = _userManager.CheckPasswordAsync(user, context.Password).GetAwaiter().GetResult();
                if (passwordOk)
                {
                    context.Result = new GrantValidationResult(user.Id, "password", null, "local", null);
                    return Task.FromResult(context.Result);
                }
            }
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "The username and password do not match", null);
            return Task.FromResult(context.Result);
        }
    }
}
