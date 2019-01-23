using System;
using System.Collections.Generic;
using System.Text;

namespace Neembly.GPIDServer.SharedClasses
{
    public class AuthTokenInfo
    {
        public string ClientId { get; set; }
        public int LifeTime { get; set; }
        public string ApiUrl { get; set; }
    }
}
