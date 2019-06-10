using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.SharedClasses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.Persistence.Interfaces
{
    public interface IDataAccess
    {
        AppUser GetAppUser(string email, string username);
        bool UserOperatorExists(string email, string username, int operatorId);
        IEnumerable<int> GetPlayersOperators(string netUserId);
        Task<int> GeneratePlayerId(string userName, string email, int operatorId);
        Task<bool> SetRegistrationStatus(string userId, RegistrationStatusNames registerStatus);
        bool EmailExists(string email, int operatorId);
    }
}
