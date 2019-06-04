using IdentityServer4;
using Neembly.GPIDServer.SharedClasses;
using Neembly.GPIDServer.SharedServices.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.SharedServices.Helpers
{
    public class TokenProviderService : ITokenProviderService
    {
        #region Member Variables
        private readonly IdentityServerTools _identityServerTools;
        private readonly AuthTokenInfo _authTokenInfo;
        #endregion

        #region Constructor
        public TokenProviderService(IdentityServerTools identityServerTools, AuthTokenInfo authTokenInfo)
        {
            _identityServerTools = identityServerTools;
            _authTokenInfo = authTokenInfo;
        }
        #endregion

        #region Actions

        #region Create Token
        public async Task<string> CreateToken()
        {
            return await _identityServerTools.IssueClientJwtAsync(_authTokenInfo.ClientId, _authTokenInfo.LifeTime, new[] { _authTokenInfo.ApiScope }, new[] { _authTokenInfo.ApiName });
        }
        #endregion

        #region Validate Token
        public async Task<bool> ValidateToken(string authToken, string issuerUrl)
        {
            return await Task.Run(() => VerifyToken(authToken, issuerUrl));
        }

        private bool VerifyToken(string authToken, string issuerUrl)
            {
            JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
            var tokenS = jwtHandler.ReadJwtToken(authToken);
            int validCounter = 0;
            if (DateTime.UtcNow > tokenS.ValidTo)
            {
                return false;
            }
            foreach (var claim in tokenS.Claims)
            {
                if (claim.Type.Equals("client_id"))
                {
                    if (claim.Value.Equals(_authTokenInfo.ClientId))
                    { validCounter++; }
                    else
                    { return false; }
                }
                if (claim.Type.Equals("iss"))
                {
                    if (claim.Value.Equals(issuerUrl))
                    { validCounter++; }
                    else
                    { return false; }
                }
                if (claim.Type.Equals("scope"))
                {
                    if (claim.Value.Equals(_authTokenInfo.ApiScope))
                    { validCounter++; }
                    else
                    { return false; }
                }
            }
            return (validCounter == 3);
        }
        #endregion
    }
    #endregion
}
