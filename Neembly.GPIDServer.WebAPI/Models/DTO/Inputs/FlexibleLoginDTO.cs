namespace Neembly.GPIDServer.WebAPI.Models.DTO.Inputs
{
    public class FlexibleLoginDTO
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
        public int OperatorId { get; set; }
    }
}