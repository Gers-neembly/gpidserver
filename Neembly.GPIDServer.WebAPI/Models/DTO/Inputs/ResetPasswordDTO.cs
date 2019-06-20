namespace Neembly.GPIDServer.WebAPI.Models.DTO.Inputs
{
    public class ResetPasswordDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string HomePage { get; set; }
        public int OperatorId { get; set; }
    }
}
