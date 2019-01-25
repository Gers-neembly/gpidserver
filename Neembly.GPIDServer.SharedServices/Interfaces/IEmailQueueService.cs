using Neembly.GPIDServer.SharedClasses;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.SharedServices.Interfaces
{
    public interface IEmailQueueService
    {
        Task<bool> Send(EmailMessage message);
    }
}
