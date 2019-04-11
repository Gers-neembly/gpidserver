using System.Collections.Generic;

namespace Neembly.GPIDServer.WebAPI.Models.Configs
{
    public class AuthClientConfiguration
    {
        public List<AuthClientInfo> AuthClientInfoList { get; set; }
        public List<AuthClientResources> AuthClientResourcesList { get; set; }
    }
}
