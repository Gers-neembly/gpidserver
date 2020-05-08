namespace Neembly.GPIDServer.WebAPI.Models.DTO.Outputs
{
    public class EmailVerificationViewModel
    {
        public string VerificationLink { get; set; }
        public string VerificationCode { get; set; }
    }
}
