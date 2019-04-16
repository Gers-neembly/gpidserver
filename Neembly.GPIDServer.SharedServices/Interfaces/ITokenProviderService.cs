using Neembly.GPIDServer.SharedClasses;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.SharedServices.Interfaces
{
    public interface ITokenProviderService
    {
        Task<string> CreateToken();
    }
}
