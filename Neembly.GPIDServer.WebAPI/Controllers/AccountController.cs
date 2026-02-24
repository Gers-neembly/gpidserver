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
using Neembly.GPIDServer.WebAPI.Filters;
using Neembly.GPIDServer.WebAPI.Models.Configs;
using Neembly.GPIDServer.WebAPI.Models.Constants.SSO;
using Neembly.GPIDServer.WebAPI.Models.DTO.Inputs;
using Neembly.GPIDServer.WebAPI.Models.DTO.Outputs;

namespace Neembly.GPIDServer.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        #region Member Variable

        private readonly UserDetailConfiguration _userDetailConfiguration;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IDataAccess _dataAccess;
        private readonly IPlayerNetService _playerNetServices;
        private readonly ITokenProviderService _tokenProviderServices;
        private readonly AuthClientConfiguration _authConfig;

        #endregion Member Variable

        #region Constructor

        public AccountController(
            UserDetailConfiguration userDetailConfiguration,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            AuthClientConfiguration authConfig,
            IConfiguration configuration,
            IDataAccess dataAccess,
            IPlayerNetService playerNetServices,
            ITokenProviderService tokenProviderServices
            )
        {
            _userDetailConfiguration = userDetailConfiguration;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _authConfig = authConfig;
            _dataAccess = dataAccess;
            _playerNetServices = playerNetServices;
            _tokenProviderServices = tokenProviderServices;
        }

        #endregion Constructor

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

        #endregion DeletePlayer

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

        #endregion UpdatePlayer

        #region Register

        [NeemblyAuthorize]
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerInfo)
        {
            AppUser user = null;
            string urlReferer = Request.Headers["Origin"].ToString();
            string userName = $"{registerInfo.UserName}_{registerInfo.OperatorId}";

            if ((!registerInfo.BoUser) && (string.IsNullOrEmpty(registerInfo.SSOAuthProvider)))
            {
                if (registerInfo.Password != registerInfo.ConfirmPassword)
                    return BadRequest(GlobalConstants.ErrPasswordsMismatch);
            }

            if (_dataAccess.UserExists(registerInfo.Email, userName, registerInfo.OperatorId))
                return BadRequest(GlobalConstants.ErrExistingAccount);

            user = _dataAccess.GetAppUser(registerInfo.Email, userName);
            string userId = string.Empty;

            int newPlayerId = await _dataAccess.GeneratePlayerId(userName, registerInfo.Email, registerInfo.OperatorId);
            registerInfo.PlayerId = newPlayerId;

            if (!string.IsNullOrEmpty(registerInfo.SSOAuthProvider))
            {
                // this is only a filler...auto password for SSO
                registerInfo.Password = $"{newPlayerId}_{registerInfo.UserName}";
                registerInfo.Password = registerInfo.Password.Length > 20 ? registerInfo.Password.Substring(0, 19) : registerInfo.Password;
            }

            if (user != null)
            {
                userId = user.Id;
            }
            else
            {
                user = new AppUser
                {
                    UserName = userName,
                    Email = registerInfo.Email,
                    DisplayUsername = registerInfo.UserName,
                    PlayerId = registerInfo.PlayerId,
                    OperatorId = registerInfo.OperatorId,
                    RegistrationStatus = Enum.GetName(typeof(RegistrationStatusNames), RegistrationStatusNames.Pending)
                };
                var result = await _userManager.CreateAsync(user, registerInfo.Password);
                if (!result.Succeeded)
                    return NotFound(GlobalConstants.ErrCreateAccount);

                var avatarImage = string.IsNullOrEmpty(registerInfo.Avatar) ? _userDetailConfiguration.AvatarInfo.DefaultUrl
                                  : registerInfo.Avatar;

                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("username", user.DisplayUsername));
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("email", user.Email));
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("registrationStatus", user.RegistrationStatus));
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("operatorId", registerInfo.OperatorId.ToString()));
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("avatarUrl", avatarImage));

                // Process SSO Auth as Claims if any
                if (!string.IsNullOrEmpty(registerInfo.SSOAuthProvider))
                {
                    Enum.TryParse(registerInfo.SSOAuthProvider.ToLower(), out AuthSSOSupported authSSOName);
                    string authProvider = SSOConstants.validSSOAuthenticator[(int)authSSOName];
                    string authProviderClaim = SSOConstants.authenticatorClaims[(int)authSSOName];
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(authProviderClaim, "true"));
                }

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
            values: new
            {
                userId = user.Id,
                code = emailConfirmationToken,
                playerId = newPlayerId,
                operatorId = registerInfo.OperatorId,
                urlreferer = urlReferer,
                urlhosted = registerInfo.HostedUrl
            },
                          protocol: Request.Scheme);
            return Ok(newPlayerId);
        }

        #endregion Register

        #region Flexible Register

        /// <summary>
        /// Flexible registration endpoint that supports either email-only or phone-only registration.
        /// This endpoint was copied from the original Register method and modified to support
        /// consolidated phone/email registration where only one contact method is required.
        ///
        /// Flow:
        /// - Email registration: Generates email verification token and callback URL (but is not used)
        /// - Phone registration: Assumes phone was already verified via OTP flow, marks as verified immediately
        /// </summary>
        [NeemblyAuthorize]
        [Route("register-flexible")]
        [HttpPost]
        public async Task<IActionResult> FlexibleRegister([FromBody] FlexibleRegisterDTO registerInfo)
        {
            try
            {
                string urlReferer = Request.Headers["Origin"].ToString();

                // Validate contact information
                var (hasEmail, hasPhone) = ValidateContactInfo(registerInfo);

                // Generate username
                string displayUsername = GenerateDisplayUsername(registerInfo, hasEmail, hasPhone);
                string userName = $"{displayUsername}_{registerInfo.OperatorId}";

                // Validate password if not SSO
                if (!registerInfo.BoUser && string.IsNullOrEmpty(registerInfo.SSOAuthProvider))
                {
                    if (registerInfo.Password != registerInfo.ConfirmPassword)
                        return BadRequest(GlobalConstants.ErrPasswordsMismatch);
                }

                // Check if user already exists
                var (userExists, existingUser) = await CheckUserExistence(registerInfo, userName, hasEmail, hasPhone);
                if (userExists)
                    return BadRequest(GlobalConstants.ErrExistingAccount);

                // Generate player ID
                string contactInfo = hasEmail ? registerInfo.Email : registerInfo.PhoneNumber;
                int newPlayerId = await _dataAccess.GeneratePlayerId(userName, contactInfo, registerInfo.OperatorId);
                registerInfo.PlayerId = newPlayerId;

                // Handle SSO password generation
                if (!string.IsNullOrEmpty(registerInfo.SSOAuthProvider))
                {
                    registerInfo.Password = $"{newPlayerId}_{displayUsername}";
                    registerInfo.Password = registerInfo.Password.Length > 20 ? registerInfo.Password.Substring(0, 19) : registerInfo.Password;
                }

                // Create or use existing user
                AppUser user = existingUser ?? await CreateNewUser(registerInfo, userName, displayUsername, hasPhone);
                string userId = user.Id;

                // Add claims for new users
                if (existingUser == null)
                {
                    await AddUserClaims(user, registerInfo, hasEmail, hasPhone);
                }

                // Handle BoUser scenario
                if (registerInfo.BoUser)
                {
                    return Ok(newPlayerId);
                }

                // Add player ID claim
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("playerId", newPlayerId.ToString()));

                // Handle verification flow
                await HandleVerificationFlow(user, registerInfo, userId, newPlayerId, hasEmail, hasPhone, urlReferer);

                return Ok(newPlayerId);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return BadRequest("Registration failed");
            }
        }

        #endregion Flexible Register

        #region Verify Email

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

        #endregion Verify Email

        #region Verify Email

        [Route("{operatorId}/verify-password")]
        [HttpGet]
        public async Task<IActionResult> VerifyPassword(int operatorId, string email, string password)
        {
            bool passwordOk = false;
            AppUser appUser = null;
            if (!string.IsNullOrEmpty(email))
            {
                if (_dataAccess.IsValidEmail(email))
                    appUser = await _dataAccess.GetAppUserOnOperator(email, operatorId); //try email on this operator
                else
                    appUser = await _dataAccess.GetAppUser($"{email}_{operatorId}"); // possible username on this operator
            }
            if (appUser == null)
                return BadRequest(GlobalConstants.ErrPlayerAccountNotExisting);
            passwordOk = await _userManager.CheckPasswordAsync(appUser, password);
            return Ok(passwordOk);
        }

        #endregion Verify Email

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

        #endregion Create User Roles

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

        #endregion Token Generator

        #region Create Player

        private async Task<bool> CreatePlayerOnProductDB(PlayerRegisterInfo playerRegister, string hostedUrl)
        {
            return await _playerNetServices.PlayerRegister(GenerateToken(hostedUrl), playerRegister);
        }

        #endregion Create Player

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

        #endregion Set Status

        #region Set Registration

        private async Task<bool> SetRegistrationStatus(string userId, RegistrationStatusNames registrationStatus)
        {
            return await _dataAccess.SetRegistrationStatus(userId, registrationStatus);
        }

        #endregion Set Registration

        #region Get Activation Link and Code

        [Route("email/verification-link-code")]
        [HttpGet]
        public async Task<IActionResult> GetVerificationLinkAndCodeAsync(int operatorId, int playerId, string operatorDomain)
        {
            var result = new EmailVerificationViewModel();
            var user = await _dataAccess.GetUserByOperatorIdAndPlayerIdAsync(operatorId, playerId);
            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var callbackUrl = $"{HttpContext.Request.Scheme}://{operatorDomain}/verify-email?userId={user.Id}&code={Uri.EscapeDataString(emailConfirmationToken)}&playerId={user.PlayerId}&operatorId={user.OperatorId}&urlreferer={string.Empty}&urlhosted={string.Empty}";
            result.VerificationCode = emailConfirmationToken;
            result.VerificationLink = callbackUrl;

            if (result == null)
                return NoContent();

            return Ok(result);
        }

        #endregion Get Activation Link and Code

        #region Flexible Register Helper Methods

        private (bool hasEmail, bool hasPhone) ValidateContactInfo(FlexibleRegisterDTO registerInfo)
        {
            bool hasEmail = !string.IsNullOrEmpty(registerInfo.Email);
            bool hasPhone = !string.IsNullOrEmpty(registerInfo.PhoneNumber);
            return (hasEmail, hasPhone);
        }

        private string GenerateDisplayUsername(FlexibleRegisterDTO registerInfo, bool hasEmail, bool hasPhone)
        {
            if (!string.IsNullOrEmpty(registerInfo.UserName))
                return registerInfo.UserName;

            if (hasEmail)
                return registerInfo.Email.Split('@')[0];

            if (hasPhone)
                return $"user{registerInfo.PhoneNumber?.Replace("+", "").Replace("-", "").Replace(" ", "")}";

            return "user";
        }

        private async Task<(bool userExists, AppUser user)> CheckUserExistence(FlexibleRegisterDTO registerInfo, string userName, bool hasEmail, bool hasPhone)
        {
            bool userExists = false;
            AppUser user = null;

            if (hasEmail)
            {
                userExists = _dataAccess.UserExists(registerInfo.Email, userName, registerInfo.OperatorId);
                user = _dataAccess.GetAppUser(registerInfo.Email, userName);
            }
            else if (hasPhone)
            {
                userExists = _dataAccess.PhoneUserExists(registerInfo.PhoneNumber, userName, registerInfo.OperatorId);
                user = await _dataAccess.GetAppUserByPhoneOnOperator(registerInfo.PhoneNumber, registerInfo.OperatorId);
            }

            return (userExists, user);
        }

        private async Task<AppUser> CreateNewUser(FlexibleRegisterDTO registerInfo, string userName, string displayUsername, bool hasPhone)
        {
            var user = new AppUser
            {
                UserName = userName,
                Email = registerInfo.Email,
                PhoneNumber = registerInfo.PhoneNumber,
                DisplayUsername = displayUsername,
                PlayerId = registerInfo.PlayerId,
                OperatorId = registerInfo.OperatorId,
                RegistrationStatus = Enum.GetName(typeof(RegistrationStatusNames), RegistrationStatusNames.Pending),
                EmailConfirmed = false,
                PhoneNumberConfirmed = hasPhone,
                TwoFactorEnabled = false,
                LockoutEnabled = true,
                AccessFailedCount = 0
            };

            var result = await _userManager.CreateAsync(user, registerInfo.Password);
            if (!result.Succeeded)
                throw new InvalidOperationException(GlobalConstants.ErrCreateAccount);

            return user;
        }

        private async Task AddUserClaims(AppUser user, FlexibleRegisterDTO registerInfo, bool hasEmail, bool hasPhone)
        {
            var avatarImage = string.IsNullOrEmpty(registerInfo.Avatar)
                ? _userDetailConfiguration.AvatarInfo.DefaultUrl
                : registerInfo.Avatar;

            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("username", user.DisplayUsername));

            // Add appropriate contact claims
            if (hasEmail)
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("email", user.Email));
            if (hasPhone)
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("phoneNumber", user.PhoneNumber));

            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("registrationStatus", user.RegistrationStatus));
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("operatorId", registerInfo.OperatorId.ToString()));
            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("avatarUrl", avatarImage));

            // Process SSO Auth as Claims if any
            if (!string.IsNullOrEmpty(registerInfo.SSOAuthProvider))
            {
                Enum.TryParse(registerInfo.SSOAuthProvider.ToLower(), out AuthSSOSupported authSSOName);
                string authProvider = SSOConstants.validSSOAuthenticator[(int)authSSOName];
                string authProviderClaim = SSOConstants.authenticatorClaims[(int)authSSOName];
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(authProviderClaim, "true"));
            }

            // Add roles if any
            if (registerInfo.Roles != null)
            {
                foreach (var roleItem in registerInfo.Roles)
                    await CreateUserRoles(user, roleItem);
            }
        }

        private async Task HandleVerificationFlow(AppUser user, FlexibleRegisterDTO registerInfo, string userId, int newPlayerId, bool hasEmail, bool hasPhone, string urlReferer)
        {
            if (hasEmail && hasPhone)
            {
                // Both email and phone - set phone as verified (assuming OTP was done), email needs verification
                user.PhoneNumberConfirmed = true;
                user.EmailConfirmed = false;
                await _userManager.UpdateAsync(user);
                await SetRegistrationStatus(userId, RegistrationStatusNames.Registered);

                await GenerateEmailConfirmation(user, newPlayerId, registerInfo.OperatorId, urlReferer, registerInfo.HostedUrl);
            }
            else if (hasEmail)
            {
                // Email only - generate verification email
                await SetRegistrationStatus(userId, RegistrationStatusNames.Registered);
                await GenerateEmailConfirmation(user, newPlayerId, registerInfo.OperatorId, urlReferer, registerInfo.HostedUrl);
            }
            else if (hasPhone)
            {
                // Phone only - already verified via OTP before reaching this endpoint
                await SetRegistrationStatus(userId, RegistrationStatusNames.Verified);
            }
        }

        private async Task GenerateEmailConfirmation(AppUser user, int playerId, int operatorId, string urlReferer, string hostedUrl)
        {
            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action(
                "verifyemail", "account",
                values: new
                {
                    userId = user.Id,
                    code = emailConfirmationToken,
                    playerId = playerId,
                    operatorId = operatorId,
                    urlreferer = urlReferer,
                    urlhosted = hostedUrl
                },
                              protocol: Request.Scheme);
        }

        #endregion Flexible Register Helper Methods

        #endregion Actions
    }
}