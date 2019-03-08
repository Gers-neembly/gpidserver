using System;
using System.Linq;
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
        private readonly IExtensionProviders _extensionProviders;
        private readonly IEmailDispatcher _emailDispatcher;
        private readonly IEmailQueueService _emailQueueService;
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
            IExtensionProviders extensionProviders,
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
            _extensionProviders = extensionProviders;
            _emailDispatcher = emailDispatcher;
            _emailQueueService = emailQueueService;
        }
        #endregion

        #region Actions

        #region Profiles
        [Route("profile")]
        [HttpPut]
        public async Task<IActionResult> Profile([FromBody] ProfileUpdateDTO profileUpdateInfo)
        {
            var dataInfo = await _dataAccess.ProfileRequestChange(profileUpdateInfo.PlayerId, new PlayerInfo
            {
                FirstName = profileUpdateInfo.PlayerInfo.FirstName,
                LastName = profileUpdateInfo.PlayerInfo.LastName,
                MobileNo = profileUpdateInfo.PlayerInfo.MobileNo,
                MobilePrefix = profileUpdateInfo.PlayerInfo.MobilePrefix
            });
            return Ok(dataInfo);
        }
        #endregion

        #region Register
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerInfo)
        {
            string clientUsername = $"{registerInfo.UserName}_{registerInfo.OperatorId}";

            if (_dataAccess.CheckEmailAccount(registerInfo.Email, registerInfo.OperatorId))
                return NotFound(GlobalConstants.ErrExistingEmailAccount);

            if (_dataAccess.CheckUsernameAccount(clientUsername, registerInfo.OperatorId))
                return NotFound(GlobalConstants.ErrExistingUsernameAccount);

            var user = new AppUser { UserName = clientUsername, Email = registerInfo.Email,
                                     DisplayUsername = registerInfo.UserName,
                                     OperatorId = registerInfo.OperatorId,
                                     RegistrationStatus = Enum.GetName(typeof(RegistrationStatusNames), RegistrationStatusNames.Pending)
                                    };

            var result = await _userManager.CreateAsync(user, registerInfo.Password);
            if (!result.Succeeded)
                return NotFound(GlobalConstants.ErrCreateAccount);

            string urlReferer = Request.Headers["Origin"].ToString();
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("username", user.DisplayUsername));
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("email", user.Email));
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("registrationStatus", user.RegistrationStatus));

            user.PlayerId = await _dataAccess.CreatePlayerById(user.Id, user.OperatorId, registerInfo.PlayerInfo);
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("operatorId", user.OperatorId.ToString()));
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("playerId", user.PlayerId));

            if (registerInfo.Roles != null)
            {
                foreach (var roleItem in registerInfo.Roles)
                    await CreateUserRoles(user, roleItem);
            }

            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                                            var callbackUrl = Url.Action(
                                            "verifyemail", "account",
                                            values: new { userId = user.Id, code = emailConfirmationToken,
                                                            operatorId = user.OperatorId, urlreferer = urlReferer,
                                                            urlhosted = registerInfo.HostedUrl},
                                            protocol: Request.Scheme);

            bool registerationCompleted = await CreatePlayerOnProductDB(
                                    new PlayerRegisterInfo
                                    {
                                        Email = user.Email,
                                        Username = user.DisplayUsername,
                                        PlayerAccountId = user.PlayerId,
                                        OperatorId = registerInfo.OperatorId,
                                        CreatedBy = user.DisplayUsername
                                    },
                                    registerInfo.HostedUrl
                                );
            if (!registerationCompleted)
                return NotFound(GlobalConstants.ErrCreateAccount);

            await SendWelcomeEmail(urlReferer, user.DisplayUsername, user.Email, user.OperatorId);
            await SendActivationEmail(callbackUrl, user.DisplayUsername, user.Email, user.OperatorId);

            return Ok();
        }
        #endregion

        #region  Verify Email 
        [Route("verifyemail")]
        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string userId, string code, string operatorId, string urlreferer, string urlhosted)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(GlobalConstants.ErrUsernameAccountNotRegistered);

            var emailConfirmationResult = await _userManager.ConfirmEmailAsync(user, code);
            if (!emailConfirmationResult.Succeeded)
                return NotFound(GlobalConstants.ErrUserAccountNotExisting);
            if (!string.IsNullOrEmpty(user.PlayerId))
            {
                await SetRegistrationStatus(userId, RegistrationStatusNames.Registered);
                await SetPlayerStatusOnProductDB(user.DisplayUsername, user.PlayerId, "Active", urlhosted);
            }
            if (string.IsNullOrEmpty(urlreferer))
                return Content($"Username: {user.DisplayUsername}, Email: {user.Email} activated. Thank you.");
            return Redirect(urlreferer);
        }
        #endregion

        #region PrivateMethods

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
            return await _extensionProviders.PlayerRegister(GenerateToken(hostedUrl), playerRegister);
        }
        #endregion

        #region Set Status
        private async Task<bool> SetPlayerStatusOnProductDB(string username, string playerId, string newStatus, string hostedUrl)
        {
            return await _extensionProviders.PlayerSetStatus(GenerateToken(hostedUrl),
                                                                new PlayerStatusInfo
                                                                {
                                                                    PlayerId = playerId,
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

        #endregion

        #endregion

    }
}