namespace Neembly.GPIDServer.SharedClasses
{
    public class SSOCheckPlayerDetails
    {
        public int PlayerId { get; set; }
        public int OperatorId { get; set; }
        public string Email { get; set; }
        public string AuthProvider { get; set; }
    }
}
