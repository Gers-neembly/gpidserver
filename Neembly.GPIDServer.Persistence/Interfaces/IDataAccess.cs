using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.SharedClasses;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.Persistence.Interfaces
{
    public interface IDataAccess
    {
        AppUser GetAppUser(string email, string username, int operatorId);
        bool CheckEmailAccount(string email, int operatorId);
        bool CheckUsernameAccount(string username, int operatorId);
        Task<string> CreatePlayerById(string userId, int operatorId, PlayerInfo playerInfo = null);
        Task<bool> SetRegistrationStatus(string userId, RegistrationStatusNames registerStatus);
        Task<bool> ProfileRequestChange(string playerId, PlayerInfo playerInfo);
    }
}
