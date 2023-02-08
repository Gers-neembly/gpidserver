using System.Security.Principal;
using Neembly.GPIDServer.SharedClasses;
using Neembly.GPIDServer.SharedServices.Interfaces;

namespace Neembly.GPIDServer.SharedServices.SSO
{
    public class SSOService : ISSOService
    {
        public SSOUserInfo GetSSOUserInfo(IPrincipal user)
        {
            return UserClaimsExtend.GetSSOUserInfo(user);
        }
    }
}
