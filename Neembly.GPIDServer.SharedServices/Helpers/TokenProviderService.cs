using IdentityServer4;
using Neembly.GPIDServer.SharedClasses;
using Neembly.GPIDServer.SharedServices.Interfaces;
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
            return null;
            //should read from configuration -- config should be loaded from the startup -- direct injection
            //return await _identityServerTools.IssueClientJwtAsync(authTokenInfo.ClientId, authTokenInfo.LifeTime, new[] { authTokenInfo.ApiName }, new[] { authTokenInfo.ApiScope });
        }
        #endregion

        #endregion


    }
}
