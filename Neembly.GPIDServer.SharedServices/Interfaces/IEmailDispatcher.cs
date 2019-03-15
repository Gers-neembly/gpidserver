using Neembly.GPIDServer.SharedClasses;
using System.Threading.Tasks;

namespace Neembly.GPIDServer.SharedServices.Interfaces
{
    public interface IEmailDispatcher
    {
        EmailMessage CreateEmailActivationLink(string emailLink, string name, string toEmail, int operatorId);
        EmailMessage CreateWelcomeEmail(string referer, string name, string toEmail, int operatorId);
        Task EmailSender(EmailMessage emailMessage);
    }
}
