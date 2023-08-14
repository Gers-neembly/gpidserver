using IdentityModel;
using IdentityServer4;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.Persistence.Interfaces;
using Neembly.GPIDServer.WebAPI.Models.Configs;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.WebAPI.Services
{
    public class IdentityClaimsProfileService : IProfileService
    {

        #region Member Variables 
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUserClaimsPrincipalFactory<AppUser> _claimsFactory;
        private readonly UserManager<AppUser> _userManager;
        private readonly IDataAccess _dataAccess;
        #endregion

        #region Constructor
        public IdentityClaimsProfileService(UserManager<AppUser> userManager, 
            IUserClaimsPrincipalFactory<AppUser> claimsFactory, 
            IHostingEnvironment hostingEnvironment,
            IDataAccess dataAccess)
        {
            _userManager = userManager;
            _claimsFactory = claimsFactory;
            _hostingEnvironment = hostingEnvironment;
            _dataAccess = dataAccess;
        }
        #endregion

        #region GetProfileData
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            var principal = await _claimsFactory.CreateAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var claims = principal.Claims.ToList();
            var operatorList = _dataAccess.GetPlayersOperators(user.Id);
            var customClaims = await _userManager.GetClaimsAsync(user);
            claims = claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type)).ToList();

            claims.Add(new Claim("email", user.Email));
            claims.Add(new Claim("username", user.DisplayUsername));
            claims.Add(new Claim("registrationStatus", user.RegistrationStatus));

            int index = 1;
            foreach (var itemOperator in operatorList)
                claims.Add(new Claim($"operator[{index++}]", itemOperator.ToString()));

            foreach (var item in customClaims)
                claims.Add(new Claim(item.Type, item.Value));

            foreach (string role in roles)
                claims.Add(new Claim(JwtClaimTypes.Role, role));
 
            context.IssuedClaims = claims;
        }
        #endregion

        #region Is Active
        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            context.IsActive = user != null;
        }
        #endregion

    }
}
