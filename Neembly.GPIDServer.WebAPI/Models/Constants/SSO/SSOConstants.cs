
namespace Neembly.GPIDServer.WebAPI.Models.Constants.SSO
{
    public enum AuthSSOSupported
    {
        google = 0,
        telegram = 1,
        facebook = 2
    }

    public static class SSOConstants
    {
        public static string[] validSSOAuthenticator = { "google", "telegram", "facebook"};
        public static string[] authenticatorClaims = { "authGoogleSSO", "authTelegramSSO", "authFacebookSSO" };
    }

}
