using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options; // Nécessaire pour IOptions
using System.Threading.Tasks;

namespace CabinetMedicalWeb.Services
{
    // CORRECTION 1 : Il faut hériter de IEmailService
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpSettings _settings;

        // CORRECTION 2 : Utiliser IOptions (avec un s)
        public SmtpEmailService(IOptions<SmtpSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password)
            };

            using var message = new MailMessage(_settings.From, to)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true // Mieux pour les emails modernes (HTML)
            };

            await client.SendMailAsync(message);
        }
    }
}