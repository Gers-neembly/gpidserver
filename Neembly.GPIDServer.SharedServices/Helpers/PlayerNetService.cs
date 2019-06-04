using IdentityServer4;
using Neembly.GPIDServer.SharedClasses;
using Neembly.GPIDServer.SharedServices.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.SharedServices.Helpers
{
    public class PlayerNetService : IPlayerNetService
    {
        #region Member Variables
        private readonly IdentityServerTools _identityServerTools;
        #endregion

        #region Constructor
        public PlayerNetService(IdentityServerTools identityServerTools)
        {
            _identityServerTools = identityServerTools;
        }
        #endregion

        #region Actions

        #region Player Register
        public async Task<bool> PlayerRegister(AuthTokenInfo authTokenInfo, PlayerRegisterInfo playerRegister)
        {
            var playerToken = await _identityServerTools.IssueClientJwtAsync(authTokenInfo.ClientId, authTokenInfo.LifeTime, new[] { authTokenInfo.ApiName }, new[] { authTokenInfo.ApiScope });
            var SysHttpClient = HttpClientSender("api/players/register", authTokenInfo.ApiUrl, playerToken, HttpTransactType.Post);
            HttpResponseMessage response = await SysHttpClient.PostAsJsonAsync<PlayerRegisterInfo>("api/players/register", playerRegister);
            return (response.StatusCode == System.Net.HttpStatusCode.OK);
        }
        #endregion

        #region Player Status
        public async Task<bool> PlayerSetStatus(AuthTokenInfo authTokenInfo, PlayerStatusInfo playerStatus)
        {
            var playerToken = await _identityServerTools.IssueClientJwtAsync(authTokenInfo.ClientId, authTokenInfo.LifeTime, new[] { authTokenInfo.ApiName }, new[] { authTokenInfo.ApiScope });
            var SysHttpClient = HttpClientSender("api/players/status", authTokenInfo.ApiUrl, playerToken, HttpTransactType.Put);
            HttpResponseMessage response = await SysHttpClient.PutAsJsonAsync<PlayerStatusInfo>("api/players/status", playerStatus);
            return (response.StatusCode == System.Net.HttpStatusCode.OK);
        }
        #endregion

        #region PrivateMethods
        public HttpClient HttpClientSender(string apiUrl, string authUrl, string token, HttpTransactType httpType)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(authUrl);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return httpClient;
        }
        #endregion

        #endregion


    }
}
