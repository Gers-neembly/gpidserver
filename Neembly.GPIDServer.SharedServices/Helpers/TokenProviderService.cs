using IdentityServer4;
using Neembly.GPIDServer.SharedClasses;
using Neembly.GPIDServer.SharedServices.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
            //should read from configuration -- config should be loaded from the startup
            return await _identityServerTools.IssueClientJwtAsync(authTokenInfo.ClientId, authTokenInfo.LifeTime, new[] { authTokenInfo.ApiName }, new[] { authTokenInfo.ApiScope });
        }
        #endregion

        #region Token Generator
        private AuthTokenInfo GenerateTokenInfo(string hostedUrl)
        {
            //should read from configuration -- config should be loaded from the startup


            //var webScope = _authConfig.AuthClientInfoList.Where(s => s.ClientId.Equals(GlobalConstants.ApiClientId, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            //return (new AuthTokenInfo
            //{
            //    ApiUrl = hostedUrl,
            //    ClientId = webScope.ClientId,
            //    LifeTime = webScope.LifeTime,
            //    ApiName = webScope.ApiScope,
            //    ApiScope = webScope.ApiScope
            //});
        }
        #endregion

        #endregion


    }
}
