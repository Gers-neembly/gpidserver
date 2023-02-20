using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
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
        const string ssoReturnPath = "sso/";
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

        #region Google Login Authentication
        [Authorize(AuthenticationSchemes = GoogleDefaults.AuthenticationScheme)]
        [Route("{operatorId}/login-google")]
        [HttpGet]
        public async Task<IActionResult> LoginGoogle(int operatorId, string returnUrl)
        {
            var authProvider = "google";
            var authProviderClaim = "authGoogleSSO";
            bool canLogin = false;
            int playerId = 0;
            string urlReferer = Request.Headers["Referer"].ToString();
            if (urlReferer.ToLower().Contains(authProvider)) urlReferer = $"https://{returnUrl}/";
            var tokenKey = await _tokenProviderService.CreateToken();

            var userClaimInfo = _ssoClaimsService.GetSSOUserInfo(this.User);
            string displayUserName = userClaimInfo.Email;
            //string displayUserName = _ssoClaimsService.GenerateUsername(this.User); // this might come handy if needed

            var emailAppUser = await _dataAccess.GetAppUserOnOperator(userClaimInfo.Email, operatorId);
            if (emailAppUser != null)
            {
                displayUserName = emailAppUser.DisplayUsername;
                playerId = emailAppUser.PlayerId;
                canLogin = await _ssoPlayerService.ProcessUserSSOClaim(authProviderClaim, emailAppUser);
            }
            else
            {
                playerId = await _dataAccess.GeneratePlayerId(displayUserName, userClaimInfo.Email, operatorId);
                var ssoRegisterInfo = new SSOPlayerRegisterInfo
                {
                    Username = displayUserName,
                    Email = userClaimInfo.Email,
                    PlayerId = playerId,
                    OperatorId = operatorId,
                    AuthProvider = authProvider,
                    AuthProviderClaim = authProviderClaim,
                    UserClaimInfo = userClaimInfo
                };
                canLogin = await _ssoPlayerService.CreateUserFromSSO(ssoRegisterInfo);
            }
            if (canLogin)
            {
                var redirectAddress = $"{urlReferer}{ssoReturnPath}{authProvider}?tokenkey={tokenKey}&username={displayUserName}&playerId={playerId}";
                var url = UriHelper.Encode(new Uri(UriHelper.Encode(new Uri(redirectAddress))));
                return Redirect(url);
            }
            else
                return Redirect($"{urlReferer}{ssoReturnPath}{authProvider}?tokenkey={tokenKey}&action=failed");
        }
        #endregion

        #region Facebook Login Authentication
        [Authorize(AuthenticationSchemes = FacebookDefaults.AuthenticationScheme)]
        [Route("{operatorId}/login-facebook")]
        [HttpGet]
        public async Task<IActionResult> LoginFacebook(int operatorId, string returnUrl)
        {
            var authProvider = "facebook";
            var authProviderClaim = "authFacebookSSO";
            bool canLogin = false;
            int playerId = 0;
            string urlReferer = Request.Headers["Referer"].ToString();
            if (urlReferer.ToLower().Contains(authProvider)) urlReferer = $"https://{returnUrl}/";
            var tokenKey = await _tokenProviderService.CreateToken();

            var userClaimInfo = _ssoClaimsService.GetSSOUserInfo(this.User);
            string displayUserName = userClaimInfo.Email;
            //string displayUserName = _ssoClaimsService.GenerateUsername(this.User); // this might come handy if needed

            var emailAppUser = await _dataAccess.GetAppUserOnOperator(userClaimInfo.Email, operatorId);
            if (emailAppUser != null)
            {
                displayUserName = emailAppUser.DisplayUsername;
                playerId = emailAppUser.PlayerId;
                canLogin = await _ssoPlayerService.ProcessUserSSOClaim(authProviderClaim, emailAppUser);
            }
            else
            {
                playerId = await _dataAccess.GeneratePlayerId(displayUserName, userClaimInfo.Email, operatorId);
                var ssoRegisterInfo = new SSOPlayerRegisterInfo
                {
                    Username = displayUserName,
                    Email = userClaimInfo.Email,
                    PlayerId = playerId,
                    OperatorId = operatorId,
                    AuthProvider = authProvider,
                    AuthProviderClaim = authProviderClaim,
                    UserClaimInfo = userClaimInfo
                };
                canLogin = await _ssoPlayerService.CreateUserFromSSO(ssoRegisterInfo);
            }
            if (canLogin)
            {
                var redirectAddress = $"{urlReferer}{ssoReturnPath}{authProvider}?tokenkey={tokenKey}&username={displayUserName}&playerId={playerId}";
                var url = UriHelper.Encode(new Uri(UriHelper.Encode(new Uri(redirectAddress))));
                return Redirect(url);
            }
            else
                return Redirect($"{urlReferer}{ssoReturnPath}{authProvider}?tokenkey={tokenKey}&action=failed");
        }
        #endregion

        #endregion


    }

}