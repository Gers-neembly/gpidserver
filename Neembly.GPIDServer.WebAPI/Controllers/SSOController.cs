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
using System.Threading.Tasks;
using Neembly.GPIDServer.WebAPI.Models.Constants.SSO;
using Neembly.GPIDServer.WebAPI.Models.oAuth;
using Neembly.GPIDServer.Security.OAuth.Telegram;
using System.Net;
using System.Linq;

namespace Neembly.GPIDServer.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SSOController : ControllerBase
    {
        private bool createPlayerAccount = false;
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
            var playerAuthResult = await this.SSOLoginAuthentication(operatorId, AuthSSOSupported.google);
            return Redirect(ConstructRedirect(returnUrl, playerAuthResult));
        }
        #endregion

        #region Telegram Login Authentication
        [Authorize(AuthenticationSchemes = TelegramAuthenticationDefaults.AuthenticationScheme)]
        [Route("{operatorId}/login-telegram")]
        [HttpGet]
        public async Task<IActionResult> LoginTelegram(int operatorId, string returnUrl)
        {
            var playerAuthResult = await this.SSOLoginAuthentication(operatorId, AuthSSOSupported.telegram);
            return Redirect(ConstructRedirect(returnUrl, playerAuthResult));
        }
        #endregion


        #region Facebook Login Authentication
        [Authorize(AuthenticationSchemes = FacebookDefaults.AuthenticationScheme)]
        [Route("{operatorId}/login-facebook")]
        [HttpGet]
        public async Task<IActionResult> LoginFacebook(int operatorId, string returnUrl)
        {
            var playerAuthResult = await this.SSOLoginAuthentication(operatorId, AuthSSOSupported.facebook);
            return Redirect(ConstructRedirect(returnUrl, playerAuthResult));
        }
        #endregion


        #region Common SSO Login Authentication
        private async Task<oAuthResult> SSOLoginAuthentication(int operatorId, AuthSSOSupported authSSOName)
        {
            var playerAuthResult = new oAuthResult();
            try
            {
                bool canLogin = false;
                int playerId = 0;

                string authProvider = SSOConstants.validSSOAuthenticator[(int)authSSOName];
                string authProviderClaim = SSOConstants.authenticatorClaims[(int)authSSOName];

                SSOUserInfo userClaimInfo = _ssoClaimsService.GetSSOUserInfo(this.User);
                string displayUserName = userClaimInfo.Email;
                string actionToTake = SSOConstants.ssoActions[(int) AuthSSOActionsToTake.createNew];
                //string displayUserName = _ssoClaimsService.GenerateUsername(this.User); // this might come handy if needed


                playerAuthResult.tokenKey = await _tokenProviderService.CreateToken();
                playerAuthResult.authProvider = authProvider;

                var emailAppUser = await _dataAccess.GetAppUserOnOperator(userClaimInfo.Email, operatorId);
                if (emailAppUser != null)
                {
                    displayUserName = emailAppUser.DisplayUsername;
                    playerId = emailAppUser.PlayerId;
                    var ssoCheckDetails = await _ssoPlayerService.CheckSSODetails(new SSOCheckPlayerDetails
                    {
                        PlayerId = emailAppUser.PlayerId,
                        Email = userClaimInfo.Email,
                        OperatorId = operatorId,
                        AuthProvider = authProvider
                    });
                    if (ssoCheckDetails.Result)
                    {
                        actionToTake = ssoCheckDetails.Action;
                        canLogin = await _ssoPlayerService.ProcessUserSSOClaim(authProviderClaim, emailAppUser);
                    }
                }
                else 
                {
                    if (createPlayerAccount)
                    {
                        string ipAddress = this.GetClientIpAddress();
                        playerId = await _dataAccess.GeneratePlayerId(displayUserName, userClaimInfo.Email, operatorId);
                        var ssoRegisterInfo = new SSOPlayerRegisterInfo
                        {
                            Username = displayUserName,
                            Email = userClaimInfo.Email,
                            PlayerId = playerId,
                            OperatorId = operatorId,
                            AuthProvider = authProvider,
                            AuthProviderClaim = authProviderClaim,
                            UserClaimInfo = userClaimInfo,
                            RegistrationIPAddress = ipAddress
                        };
                        canLogin = await _ssoPlayerService.CreateUserFromSSO(ssoRegisterInfo);
                    }
                    else
                        canLogin = true;
                }
                playerAuthResult.displayUserName = displayUserName;
                playerAuthResult.playerId = playerId;
                playerAuthResult.email = userClaimInfo.Email;
                playerAuthResult.action = actionToTake;
                playerAuthResult.result = canLogin;
            }
            catch (Exception ex)
            {
                playerAuthResult.result = false;
            }
            return playerAuthResult;
        }
        #endregion

        private string ConstructRedirect(string returnUrl, oAuthResult playerAuthResult)
        {
            string urlReferer = Request.Headers["Referer"].ToString();
            if (urlReferer.ToLower().Contains(playerAuthResult.authProvider)) urlReferer = $"https://{returnUrl}/";

            var actionToTake = !playerAuthResult.result ? "failed" : playerAuthResult.action;

            string redirectAddress = $"{urlReferer}{ssoReturnPath}{playerAuthResult.authProvider}?tokenkey={playerAuthResult.tokenKey}&email={playerAuthResult.email}";

            if (playerAuthResult.result)
                redirectAddress = $"{redirectAddress}&username={playerAuthResult.displayUserName}&playerId={playerAuthResult.playerId}";

            redirectAddress = $"{redirectAddress}&action={actionToTake}";

            return UriHelper.Encode(new Uri(UriHelper.Encode(new Uri(redirectAddress))));
        }

        #region
        private string GetClientIpAddress()
        {
            var xForwardedFor = Request.Headers["X-Forwarded-For"];
            string ipAddress = string.IsNullOrEmpty(xForwardedFor) ? Request.Headers["HTTP_X_FORWARDED_FOR"] : xForwardedFor;
            if (!string.IsNullOrEmpty(ipAddress))
            {
                var addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }
            return Request.HttpContext.Connection.RemoteIpAddress.ToString();
        }
        #endregion



        #endregion


    }

}