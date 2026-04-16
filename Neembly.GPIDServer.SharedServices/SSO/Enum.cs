using System.ComponentModel;

namespace Neembly.GPIDServer.SharedServices.SSO
{
    public class Enum
    {
        public enum SSO
        {
            [Description("AuthGoogleSSO")]
            google,
            [Description("AuthTelegramSSO")]
            telegram,
            [Description("AuthFacebookSSO")]
            facebook,
            [Description("AuthTwitterSSO")]
            twitter,
        }
        public enum TokenType
        {
            None, 
            Bearer,
            Header
        }
    }
}
