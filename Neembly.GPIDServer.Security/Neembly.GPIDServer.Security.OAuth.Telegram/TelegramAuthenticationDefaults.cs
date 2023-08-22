namespace Neembly.GPIDServer.Security.OAuth.Telegram
{
    public static class TelegramAuthenticationDefaults
    {
        public const string AuthenticationScheme = "Telegram";

        public static readonly string DisplayName = "Telegram";

        public static readonly string Issuer = "Telegram";

        public static readonly string CallbackPath = "/signin-telegram";

        public static readonly string AuthorizationEndpoint = "https://oauth.telegram.org/auth";

        public static readonly string TokenEndpoint = "https://oauth.telegram.org/token";

        public static readonly string UserInformationEndpoint = "https://oauth.telegram.org/user";
    }
}
