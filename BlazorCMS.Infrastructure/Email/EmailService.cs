using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BlazorCMS.Infrastructure.Email
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpClient = new SmtpClient
            {
                Host = _configuration["Email:SmtpServer"],
                Port = int.Parse(_configuration["Email:Port"]),
                EnableSsl = bool.Parse(_configuration["Email:EnableSSL"]),
                Credentials = new NetworkCredential(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"]
                )
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["Email:From"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);
            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
