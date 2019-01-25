using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Neembly.GPIDServer.SharedClasses;
using Neembly.GPIDServer.SharedServices.Interfaces;

namespace Neembly.GPIDServer.SharedServices.Helpers
{
    public class EmailDispatcher : IEmailDispatcher
    {
        public SmtpClient _smtpClient;
        public IConfiguration _configuration;

        public EmailDispatcher(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpClient = new SmtpClient();
            _smtpClient.Host = _configuration.GetValue<string>("Email:Smtp:Host");
            _smtpClient.Port = _configuration.GetValue<int>("Email:Smtp:Port");
            _smtpClient.Credentials = new NetworkCredential(
                    _configuration.GetValue<string>("Email:Smtp:Username"),
                    _configuration.GetValue<string>("Email:Smtp:Password")
                    );
            _smtpClient.EnableSsl = true;
        }

        public EmailMessage CreateEmailActivationLink(string emailLink, string name, string toEmail, string operatorId)
        {
            string emailBody = $"<html><body><h2>Hi {name},</h2><h1>Thank you for your registration</h1><a href = {emailLink}>Please Click to confirm your gaming account</a></body></html>";
            EmailMessage emailMessage = new EmailMessage
            {
                Sender = "info@neembly.com",
                Receipients = toEmail,
                Subject = "Activate Your Game Account",
                Message = emailBody,
                OperatorId = operatorId,
                IsHtml = true
            };
            return emailMessage;
        }

        public EmailMessage CreateWelcomeEmail(string referer, string name, string toEmail, string operatorId)
        {
            string emailBody = $"<html><body><h2>Welcome to {referer}</h2><h1>Hi {name},</h1><p>Please check your activation link that we sent you to start playing</p></body></html>";
            EmailMessage emailMessage = new EmailMessage
            {
                Sender = "info@neembly.com",
                Receipients = toEmail,
                Subject = $"Welcome {name}! from {referer}",
                Message = emailBody,
                OperatorId = operatorId,
                IsHtml = true
            };
            return emailMessage;
        }

        public async Task EmailSender(EmailMessage emailMessage)
        {
             EmailInfo emailInfo = new EmailInfo
            {
                From = emailMessage.Sender,
                To = emailMessage.Receipients,
                Subject = emailMessage.Subject,
                Body = emailMessage.Message
            };
            await SendMessage(emailInfo);
        }

        #region PrivateMethod
        private async Task SendMessage(EmailInfo emailInfo)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(emailInfo.From);
            mailMessage.To.Add(emailInfo.To);
            mailMessage.Body = emailInfo.Body;
            mailMessage.Subject = emailInfo.Subject;
            mailMessage.IsBodyHtml = true;
            await _smtpClient.SendMailAsync(mailMessage);
        }
        #endregion 
    }
}
