
namespace Neembly.GPIDServer.WebAPI.Models.Constants.SSO
{
    public enum AuthSSOSupported
    {
        google = 1,
        telegram = 2,
        facebook = 3
    }

    public static class SSOConstants
    {
        public static string[] validSSOAuthenticator = { "google", "telegram", "facebook"};
        public static string[] authenticatorClaims = { "authGoogleSSO", "authTelegramSSO", "authFacebookSSO" };
    }

}
