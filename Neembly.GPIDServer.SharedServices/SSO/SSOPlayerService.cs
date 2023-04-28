using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.SharedClasses;
using Neembly.GPIDServer.SharedServices.Interfaces;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.SharedServices.SSO
{
    public class SSOPlayerService : ISSOPlayerService
    {
        #region Member Variables
        private readonly AuthTokenInfo _authTokenInfo;
        private readonly IPlayerNetService _playerNetService;
        private readonly ILogger _logger;
        private readonly UserManager<AppUser> _userManager;
        #endregion

        #region Constructor
        public SSOPlayerService(AuthTokenInfo authTokenInfo,
                                IPlayerNetService playerNetService,
                                ILoggerFactory loggerFactory,
                                UserManager<AppUser> userManager)
        {
            _authTokenInfo = authTokenInfo;
            _playerNetService = playerNetService;
            _logger = loggerFactory.CreateLogger<SSOPlayerService>();
            _userManager = userManager;
        }
        #endregion

        #region Action

        #region Register Player
        public async Task<bool> RegisterPlayer(SSORegisterInfo registerInfo, SSOUserInfo ssoUserInfo)
        {
            try
            {
                SSOPlayerRegister playerRegister = SetSSOPlayerRegister(ssoUserInfo);
                playerRegister.PlayerId = registerInfo.PlayerId;
                playerRegister.Username = registerInfo.Username;
                playerRegister.OperatorId = registerInfo.OperatorId;
                playerRegister.SSOAuthProvider = registerInfo.AuthProvider;
                var result = await _playerNetService.PlayerSSORegister(_authTokenInfo, playerRegister);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception SSOPlayerService.RegisterPlayer", ex);
                return false;
            }
        }
        #endregion

        #region Set SSO Player Registration 
        public SSOPlayerRegister SetSSOPlayerRegister(SSOUserInfo ssoUserInfo)
        {
            return new SSOPlayerRegister
            {
                Email = ssoUserInfo.Email,
                FirstName = ssoUserInfo.FirstName,
                LastName = ssoUserInfo.LastName,
                BirthDate = ssoUserInfo.BirthDate,
                Gender = ssoUserInfo.Gender,
                MobileNumber = ssoUserInfo.MobilePhone,
                Address = ssoUserInfo.Address,
                City = ssoUserInfo.State,
                ZipCode = ssoUserInfo.PostalCode,
                Country = ssoUserInfo.Country,
                CountryCode = this.GetCountryISOCode(ssoUserInfo.Country) ?? ssoUserInfo.Country,
            };
        }
        #endregion

        #region Process User SSO Claim
        public async Task<bool> ProcessUserSSOClaim(string authProviderClaim, AppUser user)
        {
            try
            {
                var emailAppUserClaims = await _userManager.GetClaimsAsync(user);
                var test = emailAppUserClaims.Where(e => e.Type == authProviderClaim).Select(e => e.Value).FirstOrDefault();
                if (test == null)
                    await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(authProviderClaim, "true"));
                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError("Exception SSOPlayerService.ProcessUserSSOClaim", ex);
                return false;
            }
        }
        #endregion

        #region CreateUserFromSSO 
        public async Task<bool> CreateUserFromSSO(SSOPlayerRegisterInfo ssoPlayerRegisterInfo)
        {
            try
            {
                var registerInfo = new SSORegisterInfo
                {
                    PlayerId = ssoPlayerRegisterInfo.PlayerId,
                    Username = ssoPlayerRegisterInfo.Username,
                    OperatorId = ssoPlayerRegisterInfo.OperatorId,
                    AuthProvider = ssoPlayerRegisterInfo.AuthProvider
                };
                var registerResult = await this.RegisterPlayer(registerInfo, ssoPlayerRegisterInfo.UserClaimInfo);
                if (registerResult)
                {
                    AppUser user = new AppUser
                    {
                        UserName = $"{ssoPlayerRegisterInfo.Username}_{ssoPlayerRegisterInfo.OperatorId}",
                        Email = ssoPlayerRegisterInfo.Email,
                        DisplayUsername = ssoPlayerRegisterInfo.Username,
                        PlayerId = ssoPlayerRegisterInfo.PlayerId,
                        OperatorId = ssoPlayerRegisterInfo.OperatorId,
                        RegistrationStatus = System.Enum.GetName(typeof(RegistrationStatusNames), RegistrationStatusNames.Registered)
                    };
                    var result = await _userManager.CreateAsync(user, $"{user.UserName}{ssoPlayerRegisterInfo.OperatorId}");
                    if (result.Succeeded)
                    {
                        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("username", ssoPlayerRegisterInfo.Username));
                        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("email", ssoPlayerRegisterInfo.Email));
                        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("operatorId", ssoPlayerRegisterInfo.OperatorId.ToString()));
                        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("playerId", ssoPlayerRegisterInfo.PlayerId.ToString()));
                        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("registrationStatus", user.RegistrationStatus));
                        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(ssoPlayerRegisterInfo.AuthProviderClaim, "true"));
                        return true;
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError("Exception SSOPlayerService.ProcessUserSSOClaim", ex);
                return false;
            }
            return false;
        }
        #endregion

        #region Private Methods
        private string GetCountryISOCode(string country)
        {
            try
            {
                var regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(x => new RegionInfo(x.LCID));
                var englishRegion = regions.FirstOrDefault(region => region.EnglishName.Contains(country));
                var countryAbbrev = englishRegion.TwoLetterISORegionName;
                return (countryAbbrev);
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception SSOPlayerService.GetCountryISOCode", ex);
                return string.Empty;
            }
        }
        #endregion

        #endregion
    }
}
