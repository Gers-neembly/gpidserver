using Neembly.GPIDServer.SharedClasses;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.SharedServices.Interfaces
{
    public interface IEmailDispatcher
    {
        EmailMessage CreateEmailActivationLink(string emailLink, string name, string toEmail, string operatorId);
        EmailMessage CreateWelcomeEmail(string referer, string name, string toEmail, string operatorId);
        Task EmailSender(EmailMessage emailMessage);
    }
}
