using Neembly.GPIDServer.WebAPI.Models.Configs;
using System.Collections.Generic;

namespace Neembly.GPIDServer.WebAPI.Interface
{
    public interface IOperatorSSOQueries
    {
        GoogleAuthProviderData GetGoogleOperatorSSO(string socialAccountName);
        TelegramAuthProviderData GetTelegramOperatorSSO(string socialAccountName);
        FacebookAuthProviderData GetFacebookOperatorSSO(string socialAccountName);
    }
}
