using Neembly.GPIDServer.SharedClasses;
using System.Security.Principal;

namespace Neembly.GPIDServer.SharedServices.Interfaces
{
    public interface ISSOService
    {
        SSOUserInfo GetSSOUserInfo(IPrincipal user);
    }
}
