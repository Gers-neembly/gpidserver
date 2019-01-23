using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Neembly.GPIDServer.Constants;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.Persistence.Interfaces;
using Neembly.GPIDServer.SharedClasses;
using Neembly.GPIDServer.SharedServices.Interfaces;
using Neembly.GPIDServer.WebAPI.Models.DTO;

namespace Neembly.GPIDServer.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IDataAccess _dataAccess;
        private readonly IEmailDispatcher _emailDispatcher;
        private readonly IExtensionProviders _extensionProviders;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IDataAccess dataAccess,
            IEmailDispatcher emailDispatcher,
            IExtensionProviders extensionProviders
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _dataAccess = dataAccess;
            _emailDispatcher = emailDispatcher;
            _extensionProviders = extensionProviders;
        }

        [HttpPut]
        public async Task<object> Profile([FromBody] ProfileUpdateDTO profileUpdateInfo)
        {
            var resultInfo = new ResultsInfo { Success = false, DataInfo = null };

            try
            {
                if (string.IsNullOrWhiteSpace(profileUpdateInfo.PlayerId))
                {
                    resultInfo.ErrorDescription = "player is empty or null";
                    return new JsonResult(resultInfo);
                }
                var dataInfo = await _dataAccess.ProfileRequestChange(profileUpdateInfo.PlayerId, new PlayerInfo
                {
                    FirstName = profileUpdateInfo.playerInfo.FirstName,
                    LastName = profileUpdateInfo.playerInfo.LastName,
                    MobileNo = profileUpdateInfo.playerInfo.MobileNo,
                    MobilePrefix = profileUpdateInfo.playerInfo.MobilePrefix
                });
                resultInfo.DataInfo = dataInfo;
                resultInfo.Success = dataInfo;
            }
            catch (Exception ex)
            {
                resultInfo.ErrorDescription = $"{ex.Message}={ex.InnerException.Message}";
            }
            return new JsonResult(resultInfo);

        }

        [Route("register")]
        [HttpPost]
        // Description: Registers the new player, will generate a email token based link
        public async Task<object> Register([FromBody] RegisterDTO registerInfo)
        {
            var resultInfo = new ResultsInfo { Success = false, DataInfo = null };

            try
            {
                string clientOperatorId = registerInfo.OperatorId;
                if (string.IsNullOrWhiteSpace(clientOperatorId))
                {
                    resultInfo.ErrorDescription = "operatorid is null";
                    return new JsonResult(resultInfo);
                }

                if (string.IsNullOrWhiteSpace(registerInfo.Email) 
                    || string.IsNullOrWhiteSpace(registerInfo.Password)
                      || string.IsNullOrWhiteSpace(registerInfo.UserName))
                {
                    resultInfo.ErrorDescription = "email or password or username is null";
                    return new JsonResult(resultInfo);
                }

                if (registerInfo.Password != registerInfo.ConfirmPassword)
                {
                    resultInfo.ErrorDescription = "passwords don't match!";
                    return new JsonResult(resultInfo);
                }

                string clientUsername = registerInfo.UserName + '_' + clientOperatorId;

                AppUser player = _dataAccess.GetAppUser(registerInfo.Email, clientUsername, clientOperatorId);
                if (player == null)
                {
                    var user = new AppUser
                    {
                        UserName = clientUsername,
                        OperatorId = clientOperatorId,
                        Email = registerInfo.Email,
                        DisplayUsername = registerInfo.UserName,
                        RegistrationStatus = Enum.GetName(typeof(RegistrationStatusNames), RegistrationStatusNames.Pending)
                    };

                    var result = await _userManager.CreateAsync(user, registerInfo.Password);
                    if (result.Succeeded)
                    {
                        string urlReferer = Request.Headers["Origin"].ToString();
                        string theRole = string.IsNullOrEmpty(registerInfo.RoleType) ? "player" : registerInfo.RoleType.ToLower();
                        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("username", user.DisplayUsername));
                        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("email", user.Email));
                        if (theRole == "player")
                        {
                            user.PlayerId = await _dataAccess.CreatePlayerById(user.Id, user.OperatorId, registerInfo.playerInfo);
                            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("operatorId", user.OperatorId));
                            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("playerId", user.PlayerId));
                            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("registrationStatus", user.RegistrationStatus));
                        }

                        await CreateUserRoles(user, theRole);

                        var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.Action(
                            "verifyemail", "account",
                            values: new { userId = user.Id, code = emailConfirmationToken, operatorId = user.OperatorId, urlreferer = urlReferer, urlhosted = registerInfo.HostedUrl},
                            protocol: Request.Scheme);
                        resultInfo.DataInfo = callbackUrl;
                        bool registerationCompleted = await CreatePlayerOnProductDB(
                                              new PlayerRegisterInfo
                                              {
                                                  Email = user.Email,
                                                  Username = user.DisplayUsername,
                                                  PlayerAccountId = user.PlayerId,
                                                  OperatorAccountId = Convert.ToInt32(registerInfo.OperatorId)
                                              },
                                              registerInfo.HostedUrl
                                            );
                        resultInfo.Success = registerationCompleted;
                        if (registerationCompleted)
                        {
                            resultInfo.Message = $"{GlobalConstants.MsgRegisterSuccess} - {registerInfo.Email}";
                            await SendWelcomeEmail(urlReferer, user.DisplayUsername, user.Email);
                            await SendActivationEmail(callbackUrl, user.DisplayUsername, user.Email);
                        }
                        else
                            resultInfo.Message = $"{GlobalConstants.MsgRegisterFailed}";
                    }
                    else
                    {
                        var errorList = result.Errors.ToArray();
                        foreach (var error in errorList)
                        {
                            resultInfo.ErrorDescription = resultInfo.ErrorDescription + $" {error.Description}";
                        }
                    }

                }
                else
                {
                    resultInfo.ErrorDescription = "The email or username you are trying to register already exists.";
                }
            }
            catch (Exception ex)
            {
                resultInfo.ErrorDescription = $"{ex.Message}={ex.InnerException.Message}";
            }
            return new JsonResult(resultInfo);

        }

        [Route("verifyemail")]
        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string userId, string code, string operatorId, string urlreferer, string urlhosted)
        {

            var resultInfo = new ResultsInfo { Success = false, DataInfo = null };

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var emailConfirmationResult = await _userManager.ConfirmEmailAsync(user, code);
                    if (!emailConfirmationResult.Succeeded)
                    {
                        var errorList = emailConfirmationResult.Errors.ToArray();
                        foreach (var error in errorList)
                        {
                            resultInfo.ErrorDescription = resultInfo.ErrorDescription + $" {error.Description}";
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(user.PlayerId))
                        {
                            await SetRegistrationStatus(userId, RegistrationStatusNames.Registered);
                            await SetPlayerStatusOnProductDB(user.PlayerId, "Active", urlhosted);
                        }
                        return Redirect(urlreferer);
                    }
                }
                else
                {
                    resultInfo.ErrorDescription = "The player information for this verification code does not exists.";
                }

            }
            catch (Exception ex)
            {
                resultInfo.ErrorDescription = $"{ex.Message}={ex.InnerException.Message}";
            }
            return new JsonResult(resultInfo.ErrorDescription);
        }

        #region PrivateMethods
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

        private async Task<bool> CreatePlayerOnProductDB(PlayerRegisterInfo playerRegister, string hostedUrl)
        {
            AuthTokenInfo authToken = new AuthTokenInfo
            {
                ApiUrl = hostedUrl,
                ClientId = GlobalConstants.IdServerClientToken,
                LifeTime = GlobalConstants.IdServerRegisterTokenLife
            };
            return await _extensionProviders.PlayerRegister(authToken, playerRegister);
        }

        private async Task<bool> SetRegistrationStatus(string userId, RegistrationStatusNames registrationStatus)
        {
           return await _dataAccess.SetRegistrationStatus(userId, registrationStatus);
        }

        private async Task<bool> SetPlayerStatusOnProductDB(string playerId, string newStatus, string hostedUrl)
        {
            AuthTokenInfo authToken = new AuthTokenInfo
            {
                ApiUrl = hostedUrl,
                ClientId = GlobalConstants.IdServerClientToken,
                LifeTime = GlobalConstants.IdServerRegisterTokenLife
            };
            PlayerStatusInfo playerStatus = new PlayerStatusInfo
            {
                PlayerId = playerId,
                Status = newStatus
            };
            return await _extensionProviders.PlayerSetStatus(authToken, playerStatus);
        }

        private async Task SendWelcomeEmail(string referer, string name, string email)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").ToLower();
            if (environmentName != "release" 
                 || environmentName != "production")
            {
                await _emailDispatcher.SendWelcomeEmail(referer, name, email);
            }
        }

        private async Task SendActivationEmail(string content, string name, string email)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").ToLower();
            if (environmentName != "release"
                 || environmentName != "production")
            {
                await _emailDispatcher.SendActivationLink(content, name, email);
            }
        }

        #endregion

    }
}