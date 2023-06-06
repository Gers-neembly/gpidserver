using System.Collections.Generic;

namespace Neembly.GPIDServer.WebAPI.Models.Configs
{
    public class AuthClientInfo
    {
        public string ClientId { get; set; }
        public string SecretKey { get; set; }
        public string ApiScope { get; set; }
        public string Type { get; set; }
        public int LifeTime { get; set; }
        public List<string> Redirect_Uri { get; set; }
    }
}
