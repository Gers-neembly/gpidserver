using Neembly.GPIDServer.SharedClasses;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.SharedServices.Interfaces
{
    public interface IEmailDispatcher
    {
        Task SendActivationLink(string emailLink, string name, string toEmail);
        Task SendWelcomeEmail(string referer, string name, string toEmail);
    }
}
