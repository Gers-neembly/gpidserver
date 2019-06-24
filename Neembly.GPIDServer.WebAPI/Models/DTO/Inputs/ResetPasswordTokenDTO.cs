namespace Neembly.GPIDServer.WebAPI.Models.DTO.Inputs
{
    public class ResetPasswordTokenDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public int OperatorId { get; set; }
    }
}
