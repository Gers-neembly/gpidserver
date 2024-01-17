namespace Neembly.GPIDServer.SharedClasses
{
    public class SSOPlayerRegisterInfo
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public int PlayerId { get; set; }
        public int OperatorId { get; set; }
        public string AuthProvider { get; set; }
        public string AuthProviderClaim { get; set; }
        public string RegistrationIPAddress { get; set; }
        public SSOUserInfo UserClaimInfo { get; set; }
    }
}
