namespace Neembly.GPIDServer.WebAPI.Models.DTO.Inputs
{
    public class SSOPlayerRegisterInfo
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public int OperatorId { get; set; }
        public string AuthProvider { get; set; }
        public string AuthProviderClaim { get; set; }
    }
}
