namespace Neembly.GPIDServer.WebAPI.Models.oAuth
{
    public class oAuthResult
    {
        public bool result { get; set; }
        public string email { get; set; }
        public int playerId { get; set; }
        public string displayUserName { get; set; }
        public string action { get; set; }
        public string tokenKey { get; set; }
        public string authProvider { get; set; }
    }
}
