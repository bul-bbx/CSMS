using CSMS.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace CSMS.Services
{
    public class EmailService:IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var smtp = _config.GetSection("EmailSettings");
            var fromEmail = smtp["FromEmail"];
            var username = smtp["Username"];
            var password = smtp["Password"];
            var host = smtp["Host"];
            var port = int.Parse(smtp["Port"]);
            var enableSsl = bool.Parse(smtp["EnableSsl"]);

            if (string.IsNullOrEmpty(fromEmail))
                throw new Exception("FromEmail is not configured in appsettings.json");

            var mail = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mail.To.Add(to);

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl
            };

            await client.SendMailAsync(mail);
        }
    }

}
