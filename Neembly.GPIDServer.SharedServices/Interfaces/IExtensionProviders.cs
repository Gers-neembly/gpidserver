using Microsoft.AspNetCore.Mvc;
using Neembly.GPIDServer.SharedClasses;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.SharedServices.Interfaces
{
    public interface IExtensionProviders
    {
        Task<bool> PlayerRegister(AuthTokenInfo authTokenInfo, PlayerRegisterInfo playerRegister);
        Task<bool> PlayerSetStatus(AuthTokenInfo authTokenInfo, PlayerStatusInfo playerStatus);
    }
}
