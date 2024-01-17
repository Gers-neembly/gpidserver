namespace Neembly.GPIDServer.SharedClasses
{
    public class SSORegisterInfo
    {
        public int PlayerId { get; set; }
        public string Username { get; set; }
        public int OperatorId { get; set; }
        public string AuthProvider { get; set; }
        public string RegistrationIPAddress { get; set; }
    }
}
