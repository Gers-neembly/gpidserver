using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using Neembly.GPIDServer.Persistence;
using Neembly.GPIDServer.Persistence.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Neembly.GPIDServer.SharedServices.SSO.Enum;

namespace Neembly.GPIDServer.WebAPI.Validator
{
    public class CustomResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        #region Member Variable
        private readonly AppDBContext _context;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        #endregion

        public CustomResourceOwnerPasswordValidator(
            AppDBContext context,
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager
            )
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            string operatorId = context.Request.Raw["operatorId"];
            string email = context.Request.Raw["email"];
            string ssoAuthProvider = context.Request.Raw["ssoAuthProvider"];
            string userName = $"{context.UserName}_{operatorId}";
            AppUser user = null;
            if (string.IsNullOrEmpty(email))
                user = _context.Users.Where(p => p.UserName == userName).FirstOrDefault();
            else
                user = _context.Users.Where(p => p.UserName == userName
                                            && p.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (user != null)
            {
                bool passwordOk = false;
                if (string.IsNullOrEmpty(ssoAuthProvider))
                    passwordOk = _userManager.CheckPasswordAsync(user, context.Password).GetAwaiter().GetResult();
                else
                {
                    if (Enum.IsDefined(typeof(SSO), ssoAuthProvider.ToLower()))
                    {
                        var emailAppUserClaims = _userManager.GetClaimsAsync(user).GetAwaiter().GetResult();
                        var test = emailAppUserClaims.Where(e => e.Type == $"auth{ssoAuthProvider}SSO").Select(e => e.Value).FirstOrDefault();
                        passwordOk = !string.IsNullOrEmpty(test) ? test == "true" : false; 
                    }
                }
                if (passwordOk)
                {
                    _signInManager.SignInAsync(user, false);
                    context.Result = new GrantValidationResult(user.Id, "password", null, "local", null);
                    return Task.FromResult(context.Result);
                }
            }
            context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "The username and password do not match", null);
            return Task.FromResult(context.Result);
        }
    }
}
