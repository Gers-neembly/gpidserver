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
        public GoogleAuthProviderData GetGoogleOperatorSSO(string socialAccountName)
        {
            if (string.IsNullOrEmpty(socialAccountName)) return null;
            var authProvider = SSO.google.ToString();
            var operatorSSOItems =  _context.OperatorSSO
                    .Where(a => a.AuthProvider == authProvider && a.IsEnabled && a.SocialAccountName == socialAccountName)
                    .Select(a => a.Parameters).FirstOrDefault();
            return operatorSSOItems != null ? JsonConvert.DeserializeObject<GoogleAuthProviderData>(operatorSSOItems) : null;
        }
        #endregion

        #region Get Telegram Operator SSO
        public TelegramAuthProviderData GetTelegramOperatorSSO(string socialAccountName)
        {
            if (string.IsNullOrEmpty(socialAccountName)) return null;
            var authProvider = SSO.telegram.ToString();
            var operatorSSOItems = _context.OperatorSSO
                    .Where(a => a.AuthProvider == authProvider && a.IsEnabled && a.SocialAccountName == socialAccountName)
                    .Select(a => a.Parameters).FirstOrDefault();
            return operatorSSOItems != null ? JsonConvert.DeserializeObject<TelegramAuthProviderData>(operatorSSOItems) : null;
        }
        #endregion

        #region Get Facebook Operator SSO
        public FacebookAuthProviderData GetFacebookOperatorSSO(string socialAccountName)
        {
            if (string.IsNullOrEmpty(socialAccountName)) return null;
            var authProvider = SSO.facebook.ToString();
            var operatorSSOItems = _context.OperatorSSO
                    .Where(a => a.AuthProvider == authProvider && a.IsEnabled && a.SocialAccountName == socialAccountName)
                    .Select(a => a.Parameters).FirstOrDefault();
            return operatorSSOItems != null ? JsonConvert.DeserializeObject<FacebookAuthProviderData>(operatorSSOItems) : null;
        }
        #endregion

        #region Get Twitter Operator SSO
        public TwitterAuthProviderData GetTwitterOperatorSSO(string socialAccountName)
        {
            if (string.IsNullOrEmpty(socialAccountName)) return null;
            var authProvider = SSO.twitter.ToString();
            var operatorSSOItems = _context.OperatorSSO
                    .Where(a => a.AuthProvider == authProvider && a.IsEnabled && a.SocialAccountName == socialAccountName)
                    .Select(a => a.Parameters).FirstOrDefault();
            return operatorSSOItems != null ? JsonConvert.DeserializeObject<TwitterAuthProviderData>(operatorSSOItems) : null;
        }
        #endregion
    }
}
