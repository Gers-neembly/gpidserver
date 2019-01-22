using IdentityModel;
using IdentityServer4;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Neembly.GPIDServer.Persistence.Entities;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.WebAPI.Services
{
    public class IdentityClaimsProfileService : IProfileService
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUserClaimsPrincipalFactory<AppUser> _claimsFactory;
        private readonly UserManager<AppUser> _userManager;

        public IdentityClaimsProfileService(UserManager<AppUser> userManager, IUserClaimsPrincipalFactory<AppUser> claimsFactory, IHostingEnvironment hostingEnvironment)
        {
            _userManager = userManager;
            _claimsFactory = claimsFactory;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            var principal = await _claimsFactory.CreateAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var claims = principal.Claims.ToList();
            claims = claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type)).ToList();
            claims.Add(new Claim("operatorId", user.OperatorId));
            claims.Add(new Claim("playerId", user.PlayerId));
            claims.Add(new Claim("registrationStatus", user.RegistrationStatus));

            if (_hostingEnvironment.IsDevelopment() || _hostingEnvironment.EnvironmentName.ToLower() == "local")
            {
                claims.Add(new Claim("email-debugonly", user.Email));
                claims.Add(new Claim("username-debugonly", user.UserName));
            }

            foreach (string role in roles)
            {
                claims.Add(new Claim(JwtClaimTypes.Role, role));
            }

            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            context.IsActive = user != null;
        }
    }
}
