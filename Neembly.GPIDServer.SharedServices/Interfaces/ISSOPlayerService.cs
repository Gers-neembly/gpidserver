using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.SharedClasses;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.SharedServices.Interfaces
{
    public interface ISSOPlayerService
    {
        Task<bool> RegisterPlayer(SSORegisterInfo registerInfo, SSOUserInfo ssoUserInfo);
        Task<bool> ProcessUserSSOClaim(string authProviderClaim, AppUser user);
        Task<bool> CreateUserFromSSO(SSOPlayerRegisterInfo ssoPlayerRegisterInfo);
        Task<SSOCheckDetailsResult> CheckSSODetails(SSOCheckPlayerDetails checkPlayerDetails);
        SSOPlayerRegister SetSSOPlayerRegister(SSOUserInfo ssoUserInfo);
    }
}
