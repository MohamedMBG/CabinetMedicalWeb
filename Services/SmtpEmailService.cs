using System.Net;
using System.Net.Mail;
using System.Text;
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
            var from = string.IsNullOrWhiteSpace(_settings.From)
                ? _settings.Username
                : _settings.From;

            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password)
            };

            using var message = new MailMessage(from, to)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true, // Mieux pour les emails modernes (HTML)
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };

            await client.SendMailAsync(message);
        }
    }
}