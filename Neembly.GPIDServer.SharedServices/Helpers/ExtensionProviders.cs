using IdentityServer4;
using Neembly.GPIDServer.SharedClasses;
using Neembly.GPIDServer.SharedServices.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.SharedServices.Helpers
{
    public class ExtensionProviders : IExtensionProviders
    {
        private readonly IdentityServerTools _identityServerTools;
        public ExtensionProviders(IdentityServerTools identityServerTools)
        {
            _identityServerTools = identityServerTools;
        }

        public async Task<bool> PlayerRegister(AuthTokenInfo authTokenInfo, PlayerRegisterInfo playerRegister)
        {
            var playerToken = await _identityServerTools.IssueClientJwtAsync(authTokenInfo.ClientId, authTokenInfo.LifeTime);
            return await SendHttpWrite("api/players/register", authTokenInfo.ApiUrl, playerRegister, playerToken, HttpTransactType.Post);
        }

        public async Task<bool> PlayerSetStatus(AuthTokenInfo authTokenInfo, PlayerStatusInfo playerStatus)
        {
            var playerToken = await _identityServerTools.IssueClientJwtAsync(authTokenInfo.ClientId, authTokenInfo.LifeTime);
            return await SendHttpWrite("api/players/status", authTokenInfo.ApiUrl, playerStatus, playerToken, HttpTransactType.Put);
        }

        #region PrivateMethods
        public async Task<bool> SendHttpWrite(string apiUrl, string authUrl, object data, string token, HttpTransactType httpType)
        {
            var result = false;
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri($"https://{authUrl}");
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response = null;
                if (httpType == HttpTransactType.Post)
                    response = await httpClient.PostAsJsonAsync(apiUrl, data);
                else if (httpType == HttpTransactType.Put)
                   response = await httpClient.PutAsJsonAsync(apiUrl, data);
                result = (response.StatusCode == System.Net.HttpStatusCode.OK);
            }
            catch
            {
                result = false;
            }
            return result;
        }
        #endregion


    }
}
