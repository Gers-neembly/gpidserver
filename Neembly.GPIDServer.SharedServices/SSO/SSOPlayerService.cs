using IdentityServer4;
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
        #endregion

        #region Constructor
        public SSOPlayerService(AuthTokenInfo authTokenInfo,
                                IPlayerNetService playerNetService)
        {
            _authTokenInfo = authTokenInfo;
            _playerNetService = playerNetService;
        }
        #endregion

        #region Action
        public async Task<bool> RegisterPlayer(SSORegisterInfo registerInfo, SSOUserInfo ssoUserInfo)
        {
            SSOPlayerRegister playerRegister = SetSSOPlayerRegister(ssoUserInfo);
            playerRegister.PlayerId = registerInfo.PlayerId;
            playerRegister.Username = registerInfo.Username;
            playerRegister.OperatorId = registerInfo.OperatorId;
            playerRegister.SSOAuthProvider = registerInfo.AuthProvider;
            var result = await _playerNetService.PlayerSSORegister(_authTokenInfo, playerRegister);
            return result;
        }

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

        private string GetCountryISOCode(string country)
        {
            try
            {
                var regions = CultureInfo.GetCultures(CultureTypes.SpecificCultures).Select(x => new RegionInfo(x.LCID));
                var englishRegion = regions.FirstOrDefault(region => region.EnglishName.Contains(country));
                var countryAbbrev = englishRegion.TwoLetterISORegionName;
                return (countryAbbrev);
            }
            catch(Exception ex)
            {
                return string.Empty;
            }
        }
        #endregion
    }
}
