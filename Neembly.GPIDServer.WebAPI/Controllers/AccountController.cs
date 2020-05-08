using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Neembly.GPIDServer.Constants;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.Persistence.Interfaces;
using Neembly.GPIDServer.SharedClasses;
using Neembly.GPIDServer.SharedServices.Interfaces;
using Neembly.GPIDServer.WebAPI.Models.Configs;
using Neembly.GPIDServer.WebAPI.Models.DTO.Inputs;
using Microsoft.AspNetCore.Mvc.Filters;
using Neembly.GPIDServer.WebAPI.Filters;
using Microsoft.AspNetCore.Authorization;
using Neembly.GPIDServer.WebAPI.Models.DTO.Outputs;

namespace Neembly.GPIDServer.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        #region Member Variable
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IDataAccess _dataAccess;
        private readonly IPlayerNetService _playerNetServices;
        private readonly IEmailDispatcher _emailDispatcher;
        private readonly IEmailQueueService _emailQueueService;
        private readonly ITokenProviderService _tokenProviderServices;
        private readonly AuthClientConfiguration _authConfig;
        #endregion

        #region Constructor
        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            AuthClientConfiguration authConfig,
            IConfiguration configuration,
            IDataAccess dataAccess,
            IPlayerNetService playerNetServices,
            ITokenProviderService tokenProviderServices,
            IEmailDispatcher emailDispatcher,
            IEmailQueueService emailQueueService
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _authConfig = authConfig;
            _dataAccess = dataAccess;
            _playerNetServices = playerNetServices;
            _tokenProviderServices = tokenProviderServices;
            _emailDispatcher = emailDispatcher;
            _emailQueueService = emailQueueService;
        }
        #endregion

        #region Actions

        #region DeletePlayer
        [NeemblyAuthorize]
        [Route("delete")]
        [HttpPost]
        public async Task<IActionResult> DeletePlayer([FromBody] PlayerDeleteDTO playerInfo)
        {
            string url = ($"{HttpContext.Request.Scheme.ToString()}://{HttpContext.Request.Host.ToString()}");
            string token = Request.Headers["Authorization"].ToString().Substring(7);
            if (!await _tokenProviderServices.ValidateToken(token, url))
            {
                return Unauthorized();
            }
            string userName = $"{playerInfo.Username}_{playerInfo.OperatorId}";
            AppUser ppUser = _dataAccess.GetAppUser(playerInfo.Email, userName);
            var result = await _userManager.DeleteAsync(ppUser);
            return Ok();
        }
        #endregion

        #region UpdatePlayer
        [NeemblyAuthorize]
        [Route("edit")]
        [HttpPost]
        public async Task<IActionResult> UpdatePlayer([FromBody] PlayerUpdateDTO playerInfo)
        {
            string url = ($"{HttpContext.Request.Scheme.ToString()}://{HttpContext.Request.Host.ToString()}");
            string token = Request.Headers["Authorization"].ToString().Substring(7);
            if (!await _tokenProviderServices.ValidateToken(token, url))
            {
                return Unauthorized();
            }
            string userName = $"{playerInfo.Username}_{playerInfo.OperatorId}";
            if (_dataAccess.EmailExists(playerInfo.Email, playerInfo.OperatorId, playerInfo.PlayerId))
            {
                return BadRequest(GlobalConstants.ErrPlayerExistingAccount);
            }

            //insert update function here
            AppUser ppUser = _dataAccess.GetAppUser(userName, playerInfo.PlayerId);
            if (ppUser == null) return NotFound(GlobalConstants.ErrPlayerAccountNotExisting);
            ppUser.Email = playerInfo.Email;
            var result = await _userManager.UpdateAsync(ppUser);
            return Ok();
        }
        #endregion

        #region Register
        [NeemblyAuthorize]
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerInfo)
        {
            AppUser user = null;
            string urlReferer = Request.Headers["Origin"].ToString();
            string userName = $"{registerInfo.UserName}_{registerInfo.OperatorId}";

            if (!registerInfo.BoUser)
            {
                if (registerInfo.Password != registerInfo.ConfirmPassword)
                    return BadRequest(GlobalConstants.ErrPasswordsMismatch);
            }

            if (_dataAccess.UserExists(registerInfo.Email, userName, registerInfo.OperatorId))
                return BadRequest(GlobalConstants.ErrExistingAccount);

            user = _dataAccess.GetAppUser(registerInfo.Email, userName);
            string userId = string.Empty;

            int newPlayerId = await _dataAccess.GeneratePlayerId(userName,  registerInfo.Email, registerInfo.OperatorId);
            registerInfo.PlayerId = newPlayerId;

            if (user != null)
                {
                    userId = user.Id;
                }
            else
            {
                user = new AppUser { UserName = userName, Email = registerInfo.Email,
                                        DisplayUsername = registerInfo.UserName,
                                        PlayerId = registerInfo.PlayerId,
                                        OperatorId = registerInfo.OperatorId,
                                        RegistrationStatus = Enum.GetName(typeof(RegistrationStatusNames), RegistrationStatusNames.Pending)
                                   };
                var result = await _userManager.CreateAsync(user, registerInfo.Password);
                if (!result.Succeeded)
                    return NotFound(GlobalConstants.ErrCreateAccount);

                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("username", user.DisplayUsername));
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("email", user.Email));
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("registrationStatus", user.RegistrationStatus));
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("operatorId", registerInfo.OperatorId.ToString()));

                if (registerInfo.Roles != null)
                {
                    foreach (var roleItem in registerInfo.Roles)
                        await CreateUserRoles(user, roleItem);
                }
                userId = user.Id;
            }

            if (registerInfo.BoUser)
            {
                return Ok(newPlayerId);
            }
            if (user != null)
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("playerId", newPlayerId.ToString()));

            await SetRegistrationStatus(userId, RegistrationStatusNames.Registered);

            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                                            var callbackUrl = Url.Action(
                                            "verifyemail", "account",
                                            values: new { userId = user.Id, code = emailConfirmationToken,
                                                          playerId = newPlayerId, operatorId = registerInfo.OperatorId,
                                                          urlreferer = urlReferer, urlhosted = registerInfo.HostedUrl},
                                                          protocol: Request.Scheme);

            //Remove due to NEEM-1053
            //Email is no longer supported in ID Server
            //await SendWelcomeEmail(urlReferer, user.DisplayUsername, user.Email, registerInfo.OperatorId);
            //await SendActivationEmail(callbackUrl, user.DisplayUsername, user.Email, registerInfo.OperatorId);

            return Ok(newPlayerId);
        }
        #endregion

        #region  Verify Email 
        [Route("verifyemail")]
        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string userId, string code, int playerId, int operatorId, string urlreferer, string urlhosted)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(GlobalConstants.ErrUsernameAccountNotRegistered);

            var emailConfirmationResult = await _userManager.ConfirmEmailAsync(user, code);
            if (!emailConfirmationResult.Succeeded)
                return NotFound(GlobalConstants.ErrUserAccountNotExisting);
            if (operatorId > 0)
                await SetRegistrationStatus(userId, RegistrationStatusNames.Verified);
            if (string.IsNullOrEmpty(urlreferer))
                return Content($"Username: {user.DisplayUsername}, Email: {user.Email} activated. Thank you.");
            return Redirect(urlreferer);
        }
        #endregion

        #region Create User Roles
        private async Task CreateUserRoles(AppUser user, string roleDesired)
        {
            IdentityResult roleResult;
            var roleCheck = await _roleManager.RoleExistsAsync(roleDesired);
            if (!roleCheck)
            {
                roleResult = await _roleManager.CreateAsync(new IdentityRole(roleDesired));
            }
            await _userManager.AddToRoleAsync(user, roleDesired);
        }
        #endregion

        #region Token Generator
        private AuthTokenInfo GenerateToken(string hostedUrl)
        {
            var ppWebScope = _authConfig.AuthClientInfoList.Where(s => s.ClientId.Equals(GlobalConstants.ApiClientId, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            return (new AuthTokenInfo
            {
                ApiUrl = hostedUrl,
                ClientId = ppWebScope.ClientId,
                LifeTime = ppWebScope.LifeTime,
                ApiName = ppWebScope.ApiScope,
                ApiScope = ppWebScope.ApiScope
            });
        }
        #endregion

        #region Create Player 
        private async Task<bool> CreatePlayerOnProductDB(PlayerRegisterInfo playerRegister, string hostedUrl)
        {
            return await _playerNetServices.PlayerRegister(GenerateToken(hostedUrl), playerRegister);
        }
        #endregion

        #region Set Status
        private async Task<bool> SetPlayerStatusOnProductDB(string username, int playerId, int operatorId, string newStatus, string hostedUrl)
        {
            return await _playerNetServices.PlayerSetStatus(GenerateToken(hostedUrl),
                                                                new PlayerStatusInfo
                                                                {
                                                                    PlayerId = playerId,
                                                                    OperatorId = operatorId,
                                                                    Status = newStatus,
                                                                    ModifiedBy = username
                                                                });
        }
        #endregion

        #region Set Registration 
        private async Task<bool> SetRegistrationStatus(string userId, RegistrationStatusNames registrationStatus)
        {
            return await _dataAccess.SetRegistrationStatus(userId, registrationStatus);
        }
        #endregion

        #region Welcome Email
        private async Task SendWelcomeEmail(string referer, string name, string email, int operatorId)
        {   
            var emailMessage = _emailDispatcher.CreateWelcomeEmail(referer, name, email, operatorId);
            await _emailQueueService.Send(emailMessage);

            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").ToLower();
            if (environmentName != "release" 
                 || environmentName != "production")
            {
                await _emailDispatcher.EmailSender(emailMessage);
            }
        }
        #endregion

        #region Activation Email
        private async Task SendActivationEmail(string content, string name, string email, int operatorId)
        {
            var emailMessage = _emailDispatcher.CreateEmailActivationLink(content, name, email, operatorId);
            await _emailQueueService.Send(emailMessage);

            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").ToLower();
            if (environmentName != "release"
                 || environmentName != "production")
            {
                await _emailDispatcher.EmailSender(emailMessage);
            }
        }
        #endregion

        #region Get Activation Link and Code
        [Route("email/verification-link-code")]
        [HttpGet]
        public async Task<IActionResult> GetVerificationLinkAndCodeAsync(int operatorId, int playerId)
        {
            var result = new EmailVerificationViewModel();
            var user = await _dataAccess.GetUserByOperatorIdAndPlayerIdAsync(operatorId, playerId);
            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action("verifyemail", "account",
                values: new
                {
                    userId = user.Id,
                    code = emailConfirmationToken,
                    playerId = user.PlayerId,
                    operatorId = user.OperatorId,
                    urlreferer = string.Empty,
                    urlhosted = string.Empty
                },
                protocol: Request.Scheme);

            result.VerificationCode = emailConfirmationToken;
            result.VerificationLink = callbackUrl;

            if (result == null)
                return NoContent();

            return Ok(result);
        }
        #endregion
        #endregion
    }
}