using System.Collections.Generic;

namespace Neembly.GPIDServer.WebAPI.Models.Configs
{
    public class AuthClientResources
    {
        public string Name { get; set; }
        public string Request { get; set; }
        public string SecretKey { get; set; }
    }

    public class AuthorityHostItem
    {
        public string webname { get; set; }
        public string webaddress { get; set; }
    }

    public class AuthorityHost
    {
        public List<AuthorityHostItem> AuthorityHostList { get; set; }
    }

}
