using System.ComponentModel;

namespace Neembly.GPIDServer.SharedServices.SSO
{
    public class Enum
    {
        public enum SSO
        {
            [Description("AuthGoogleSSO")]
            Google,
            [Description("AuthFacebookSSO")]
            Facebook,
        }
        public enum TokenType
        {
            None, 
            Bearer,
            Header
        }
    }
}
