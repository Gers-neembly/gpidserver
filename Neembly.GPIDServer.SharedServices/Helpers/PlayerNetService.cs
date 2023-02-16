using IdentityServer4;
using Neembly.GPIDServer.SharedClasses;
using Neembly.GPIDServer.SharedServices.Interfaces;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static Neembly.GPIDServer.SharedServices.SSO.Enum;

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
            try
            {
                var endpoint = "api/players/register";
                var playerToken = await _identityServerTools.IssueClientJwtAsync(authTokenInfo.ClientId, authTokenInfo.LifeTime, new[] { authTokenInfo.ApiName }, new[] { authTokenInfo.ApiScope });
                var SysHttpClient = HttpClientSender($"{endpoint}", authTokenInfo.ApiUrl, playerToken);
                string urlAddress = $"{authTokenInfo.ApiUrl}/{endpoint}";
                var jsonRequestString = JsonConvert.SerializeObject(playerRegister);
                var content = new StringContent(jsonRequestString, Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = (await SysHttpClient.PostAsync(urlAddress, content));
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseData = await response.Content.ReadAsAsync<RegisterResult>();
                    if (responseData != null)
                        return responseData.Success;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region Player SSO Register
        public async Task<bool> PlayerSSORegister(AuthTokenInfo authTokenInfo, SSOPlayerRegister playerRegister)
        {
            try
            {
                var endpoint = $"api/sso/{playerRegister.OperatorId}/register";
                var playerToken = await _identityServerTools.IssueClientJwtAsync(authTokenInfo.ClientId, authTokenInfo.LifeTime, new[] { authTokenInfo.ApiName }, new[] { authTokenInfo.ApiScope });
                var SysHttpClient = HttpClientSender($"{endpoint}", authTokenInfo.ApiUrl, playerToken, TokenType.Header);
                string urlAddress = $"{authTokenInfo.ApiUrl}/{endpoint}";
                var jsonRequestString = JsonConvert.SerializeObject(playerRegister);
                var content = new StringContent(jsonRequestString, Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = (await SysHttpClient.PostAsync(urlAddress, content));
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseData = await response.Content.ReadAsAsync<SSORegisterResult>();
                    if (responseData != null)
                        return responseData.Success;
                }
                return false;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        #endregion


        #region Player Status
        public async Task<bool> PlayerSetStatus(AuthTokenInfo authTokenInfo, PlayerStatusInfo playerStatus)
        {
            var playerToken = await _identityServerTools.IssueClientJwtAsync(authTokenInfo.ClientId, authTokenInfo.LifeTime, new[] { authTokenInfo.ApiName }, new[] { authTokenInfo.ApiScope });
            var SysHttpClient = HttpClientSender("api/players/status", authTokenInfo.ApiUrl, playerToken);
            HttpResponseMessage response = await SysHttpClient.PutAsJsonAsync<PlayerStatusInfo>("api/players/status", playerStatus);
            return (response.StatusCode == System.Net.HttpStatusCode.OK);
        }
        #endregion

        #region PrivateMethods
        public HttpClient HttpClientSender(string apiUrl, string authUrl, string token, TokenType tokenType = TokenType.Bearer)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(authUrl);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            switch(tokenType) {
                case TokenType.Bearer: httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); break;
                case TokenType.Header: httpClient.DefaultRequestHeaders.Add("SystemToken", token); break;
            }
            return httpClient;
        }
        #endregion

        #endregion


    }
}
