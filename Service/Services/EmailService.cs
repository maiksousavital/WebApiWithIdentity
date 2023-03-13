using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Service.Dtos;
using Service.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;

namespace Service.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEmail(EmailDto emailDto)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailConfiguration:From").Value));
            email.To.Add(MailboxAddress.Parse(emailDto.To));
            email.Subject = emailDto.Subject;
            email.Body = new TextPart(TextFormat.Html) { Text = emailDto.Body };

            using var smtp = new SmtpClient();
            smtp.Connect(_configuration.GetSection("EmailConfiguration:SmtpServer").Value,
                Int32.Parse(_configuration.GetSection("EmailConfiguration:Port").Value),
                SecureSocketOptions.StartTls);

            smtp.Authenticate(_configuration.GetSection("EmailConfiguration:Username").Value,
                _configuration.GetSection("EmailConfiguration:Password").Value);

            var returnEmail = smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
