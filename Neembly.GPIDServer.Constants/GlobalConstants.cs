namespace Neembly.GPIDServer.Constants
{
    public static class GlobalConstants
    {
        #region Message Codes
        public const string ErrRegUsername = "REGV-001";
        public const string ErrPasswordValue = "REGV-002";
        public const string ErrPasswordLength = "REGV-003";
        public const string ErrConfirmPasswordValue = "REGV-004";
        public const string ErrConfirmPasswordLength = "REGV-006";
        public const string ErrPasswordsMismatch = "REGV-008";
        public const string ErrEmailValue = "REGV-010";
        public const string ErrEmailFormat = "REGV-011";
        public const string ErrCreateAccount = "REGD-001";
        public const string ErrExistingEmailAccount = "REGD-002";
        public const string ErrExistingUsernameAccount = "REGD-003";
        public const string ErrUsernameAccountNotRegistered = "LOGD-001";
        public const string ErrUserAccountNotExisting = "LOGD-003";
        #endregion

        #region Auth
        public const string ApiScope = "Neembly.GP.Web.PlayerPortalApi";
        public const string ApiClientId = "Neembly.PlayerPortalApi.Services";
        public const string AuthTypePassword = "passwordGrant";
        public const string AuthTypeClientCredentials = "clientCredentials";
        #endregion

    }
}
