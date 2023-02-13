using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.Persistence.Interfaces;
using Neembly.GPIDServer.SharedClasses;
using Neembly.GPIDServer.SharedServices.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SSOController : ControllerBase
    {
        #region Member Variable
        private readonly IDataAccess _dataAccess;
        private readonly ISSOClaimsService _ssoClaimsService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        #endregion

        #region Constructor
        public SSOController(
            IDataAccess dataAccess,
            ISSOClaimsService ssoClaimsService,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager)
        {
            _dataAccess = dataAccess;
            _ssoClaimsService = ssoClaimsService;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        #endregion

        #region Actions
        [Authorize(AuthenticationSchemes = GoogleDefaults.AuthenticationScheme)]
        [Route("{operatorId}/login-google")]
        [HttpGet]
        public async Task<IActionResult> Get(int operatorId)
        {
            bool canLogin = false;
            AppUser user = null;
            int playerId = 0;
            string urlReferer = Request.Headers["WinkaHost"].ToString();

            var userClaimInfo = _ssoClaimsService.GetSSOUserInfo(this.User);

            var emailAppUser = await _dataAccess.GetAppUserOnOperator(userClaimInfo.Email, operatorId);
            if (emailAppUser != null)
            {
                var emailAppUserClaims = await _userManager.GetClaimsAsync(emailAppUser);
                playerId = emailAppUser.PlayerId;
                var test = emailAppUserClaims.Where(e => e.Type == "authGoogleSSO").Select(e => e.Value).FirstOrDefault();
                if (test == null)
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("authGoogleSSO", "true"));
                canLogin = true;
            }
            else
            {
                string userName = _ssoClaimsService.GenerateUsername(this.User);
                playerId = await _dataAccess.GeneratePlayerId(userName, userClaimInfo.Email, operatorId);
                user = new AppUser
                {
                    UserName = $"{userName}_{operatorId}",
                    Email = userClaimInfo.Email,
                    DisplayUsername = userName,
                    PlayerId = playerId,
                    OperatorId = operatorId,
                    RegistrationStatus = Enum.GetName(typeof(RegistrationStatusNames), RegistrationStatusNames.Registered)
                };
                var result = await _userManager.CreateAsync(user, $"{user.UserName}{operatorId}");
                if (result.Succeeded)
                {
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("username", user.DisplayUsername));
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("email", user.Email));
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("operatorId", operatorId.ToString()));
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("playerId", playerId.ToString()));
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("registrationStatus", user.RegistrationStatus));
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("authGoogleSSO", "true"));
                    canLogin = true;
                }
            }
            if (canLogin)
                return Redirect($"{urlReferer}/sso/google?username={user.UserName}&playerId={playerId}");
            else
                return Redirect($"{urlReferer}");
        }

        #endregion


    }

}