namespace Neembly.GPIDServer.Constants
{
    public static class GlobalConstants
    {
        #region IdServer
        public const string IdServerClient = "ro.gpidserver";
        public const string IdServerClientToken = "pp.gpidserver";
        public const string IdServerSecret = "secret";
        public const string IdServerApiScope = "api1";
        public const string IdServerApiAudience = "api1";
        public const int IdServerRegisterTokenLife = 3600;
        #endregion

        #region Messages
        public const string MsgRegisterSuccess = "Registration completed, please verify your email";
        public const string MsgRegisterFailed = "Registration failed, player not created";
        #endregion

    }
}
