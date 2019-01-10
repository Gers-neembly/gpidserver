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

        public async Task SendActivationLink(string emailLink, string name, string toEmail)
        {
            string emailBody = $"<html><body><h2>Hi {name},</h2><h1>Thank you for your registration</h1><a href = {emailLink}>Please Click to confirm your gaming account</a></body></html>";
             EmailInfo emailInfo = new EmailInfo
            {
                From = "info@neembly.com",
                To = toEmail,
                Subject = "Activate Your Game Account",
                Body = emailBody
            };
            await SendMessage(emailInfo);
        }

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
    }
}
