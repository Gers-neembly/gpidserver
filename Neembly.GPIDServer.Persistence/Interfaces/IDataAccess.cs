using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.SharedClasses;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.Persistence.Interfaces
{
    public interface IDataAccess
    {
        AppUser GetAppUser(string email, string username, string operatorId);
        Task<string> CreatePlayerById(string userId, string operatorId, PlayerInfo playerInfo = null);
        Task<bool> SetRegistrationStatus(string userId, RegistrationStatusNames registerStatus);
    }
}
