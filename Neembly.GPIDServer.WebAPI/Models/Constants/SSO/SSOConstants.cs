
namespace Neembly.GPIDServer.WebAPI.Models.Constants.SSO
{
    public enum AuthSSOSupported
    {
        google = 0,
        telegram = 1,
        facebook = 2
    }

    public enum AuthSSOActionsToTake
    {
        createNew = 0,  
        authenticate = 1,
        connected = 2
        // createNew - the email is not present therefore, call register
        // authenticate - this is a registered player but is not connected to this email account (show connect account dialog)
        // connected - this is a regsitered player and is a connected now just login the player  
    }

    public static class SSOConstants
    {
        public static string[] validSSOAuthenticator = { "google", "telegram", "facebook"};
        public static string[] authenticatorClaims = { "authGoogleSSO", "authTelegramSSO", "authFacebookSSO" };

        public static string[] ssoActions = { "create-new", "authenticate", "connected" };

    }

}
