using Neembly.GPIDServer.SharedClasses;
using System.Security.Principal;

namespace Neembly.GPIDServer.SharedServices.Interfaces
{
    public interface ISSOClaimsService
    {
        SSOUserInfo GetSSOUserInfo(IPrincipal user);
        string GenerateUsername(IPrincipal user);
    }
}
