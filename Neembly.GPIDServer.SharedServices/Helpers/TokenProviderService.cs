using IdentityServer4;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Neembly.GPIDServer.SharedClasses;
using Neembly.GPIDServer.SharedServices.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
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
            //bool validToken=ValidateToken(await _identityServerTools.IssueClientJwtAsync(_authTokenInfo.ClientId, _authTokenInfo.LifeTime, new[] { _authTokenInfo.ApiScope }, new[] { _authTokenInfo.ApiName }));
            return await _identityServerTools.IssueClientJwtAsync(_authTokenInfo.ClientId, _authTokenInfo.LifeTime, new[] { _authTokenInfo.ApiScope }, new[] { _authTokenInfo.ApiName });
        }
        #endregion

        //private const string X509Cert = "THE_VALUE_YOU_DON'T_GET_TO_SEE";
        //public static X509Certificate2 DefaultCert_Public_2048 = new X509Certificate2(Convert.FromBase64String(X509Cert));
        //public static X509SecurityKey DefaultX509Key_Public_2048 = new X509SecurityKey(DefaultCert_Public_2048);
        //public static SigningCredentials DefaultX509SigningCreds_Public_2048_RsaSha2_Sha2 = new SigningCredentials(DefaultX509Key_Public_2048, SecurityAlgorithms.RsaSha256Signature);

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
}
