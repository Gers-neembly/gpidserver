namespace Neembly.GPIDServer.WebAPI.Models.DTO.Inputs
{
    public class ChangePasswordDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public int OperatorId { get; set; }
    }
}
