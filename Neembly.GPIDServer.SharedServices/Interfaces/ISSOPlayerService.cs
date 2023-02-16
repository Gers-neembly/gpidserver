
using Neembly.GPIDServer.SharedClasses;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.SharedServices.Interfaces
{
    public interface ISSOPlayerService
    {
        Task<bool> RegisterPlayer(SSORegisterInfo registerInfo, SSOUserInfo ssoUserInfo);
        SSOPlayerRegister SetSSOPlayerRegister(SSOUserInfo ssoUserInfo);
    }
}
