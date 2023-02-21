using Neembly.GPIDServer.Persistence;
using Neembly.GPIDServer.WebAPI.Interface;
using Neembly.GPIDServer.WebAPI.Models.Configs;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using static Neembly.GPIDServer.SharedServices.SSO.Enum;

namespace Neembly.GPIDServer.WebAPI.Queries
{
    public class OperatorSSOQueries : IOperatorSSOQueries
    {
        #region Member Variables
        private AppDBContext _context;
        #endregion

        #region Constructor
        public OperatorSSOQueries(AppDBContext context)
        {
            _context = context;
        }
        #endregion


        #region Get Google Operator SSO
        public List<GoogleAuthProviderData> GetGoogleOperatorSSO()
        {
            var authProvider = SSO.google.ToString();
            var operatorSSOItems =  _context.OperatorSSO
                    .Where(a => a.AuthProvider == authProvider && a.IsEnabled)
                    .Select(a => a.Parameters);
            var AuthData = new List<GoogleAuthProviderData>();
            foreach (var ssoItem in operatorSSOItems)
                AuthData.Add(JsonConvert.DeserializeObject<GoogleAuthProviderData>(ssoItem));
            return AuthData;
        }
        #endregion

        #region Get Facebook Operator SSO
        public List<FacebookAuthProviderData> GetFacebookOperatorSSO()
        {
            var authProvider = SSO.facebook.ToString();
            var operatorSSOItems = _context.OperatorSSO
                    .Where(a => a.AuthProvider == authProvider && a.IsEnabled)
                    .Select(a => a.Parameters);
            var AuthData = new List<FacebookAuthProviderData>();
            foreach (var ssoItem in operatorSSOItems)
                AuthData.Add(JsonConvert.DeserializeObject<FacebookAuthProviderData>(ssoItem));
            return AuthData;
        }
        #endregion
    }
}
