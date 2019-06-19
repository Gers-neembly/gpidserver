namespace Neembly.GPIDServer.WebAPI.Models.DTO.Inputs
{
    public class ResetPasswordAutoTokenDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public int OperatorId { get; set; }
    }
}
