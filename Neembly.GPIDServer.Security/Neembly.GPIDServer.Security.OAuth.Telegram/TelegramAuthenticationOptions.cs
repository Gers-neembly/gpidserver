
using Microsoft.AspNetCore.Authentication.OAuth;

namespace Neembly.GPIDServer.Security.OAuth.Telegram
{
    public class TelegramAuthenticationOptions : OAuthOptions
    {
        public string Bot_Id { get; set; }
        public string Public_Key { get; set; }
        public string Nonce { get; set; }
        public TelegramAuthenticationOptions()
        {
            ClaimsIssuer = TelegramAuthenticationDefaults.Issuer;
            CallbackPath = TelegramAuthenticationDefaults.CallbackPath;
            AuthorizationEndpoint = TelegramAuthenticationDefaults.AuthorizationEndpoint;
            TokenEndpoint = TelegramAuthenticationDefaults.TokenEndpoint;
            UserInformationEndpoint = TelegramAuthenticationDefaults.UserInformationEndpoint;
        }
    }
}
