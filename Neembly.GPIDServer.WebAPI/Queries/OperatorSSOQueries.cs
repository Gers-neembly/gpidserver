using Neembly.GPIDServer.Persistence;
using Neembly.GPIDServer.WebAPI.Interface;
using Neembly.GPIDServer.WebAPI.Models.Configs;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

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


        public List<GoogleAuthProviderData> GetperatorSSOByProvider(string authProvider)
        {
            var operatorSSOItems =  _context.OperatorSSO
                    .Where(a => a.AuthProvider == authProvider && a.IsEnabled)
                    .Select(a => a.Parameters);
            var googleAuthData = new List<GoogleAuthProviderData>();
            foreach (var ssoItem in operatorSSOItems)
                googleAuthData.Add(JsonConvert.DeserializeObject<GoogleAuthProviderData>(ssoItem));
            return googleAuthData;
        }
    }
}
