using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.Persistence.Interfaces;
using Neembly.GPIDServer.SharedClasses;
using Neembly.GPIDServer.SharedServices.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Neembly.GPIDServer.SharedServices.SSO.Enum;

namespace Neembly.GPIDServer.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SSOController : ControllerBase
    {
        const string ssoReturnPath = "sso/google";
        #region Member Variable
        private readonly IDataAccess _dataAccess;
        private readonly ITokenProviderService _tokenProviderService;
        private readonly ISSOClaimsService _ssoClaimsService;
        private readonly ISSOPlayerService _ssoPlayerService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        #endregion

        #region Constructor
        public SSOController(
            IDataAccess dataAccess,
            ITokenProviderService tokenProviderService,
            ISSOClaimsService ssoClaimsService,
            ISSOPlayerService ssoPlayerService,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager)
        {
            _dataAccess = dataAccess;
            _tokenProviderService = tokenProviderService;
            _ssoClaimsService = ssoClaimsService;
            _ssoPlayerService = ssoPlayerService;
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
            string urlReferer = Request.Headers["Referer"].ToString();
            var tokenKey = await _tokenProviderService.CreateToken();

            var userClaimInfo = _ssoClaimsService.GetSSOUserInfo(this.User);
            string displayUserName = _ssoClaimsService.GenerateUsername(this.User);

            var emailAppUser = await _dataAccess.GetAppUserOnOperator(userClaimInfo.Email, operatorId);
            if (emailAppUser != null)
            {
                displayUserName = emailAppUser.DisplayUsername;
                var emailAppUserClaims = await _userManager.GetClaimsAsync(emailAppUser);
                playerId = emailAppUser.PlayerId;
                var test = emailAppUserClaims.Where(e => e.Type == "authGoogleSSO").Select(e => e.Value).FirstOrDefault();
                if (test == null)
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("authGoogleSSO", "true"));
                canLogin = true;
            }
            else
            {
                playerId = await _dataAccess.GeneratePlayerId(displayUserName, userClaimInfo.Email, operatorId);
                var registerInfo = new SSORegisterInfo {
                    PlayerId = playerId,
                    Username = displayUserName,
                    OperatorId = operatorId,
                    AuthProvider = SSO.Google.ToString()
                };
                var registerResult = await _ssoPlayerService.RegisterPlayer(registerInfo, userClaimInfo);
                if (registerResult)
                {
                    user = new AppUser
                    {
                        UserName = $"{displayUserName}_{operatorId}",
                        Email = userClaimInfo.Email,
                        DisplayUsername = displayUserName,
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
            }
            if (canLogin)
            {
                var redirectAddress = $"{urlReferer}{ssoReturnPath}?tokenkey={tokenKey}&username={displayUserName}&playerId={playerId}";
                var url = UriHelper.Encode(new Uri(UriHelper.Encode(new Uri(redirectAddress))));
                return Redirect(url);
            }
            else
                return Redirect($"{urlReferer}{ssoReturnPath}?tokenkey={tokenKey}&action=failed");
        }

        #endregion


    }

}