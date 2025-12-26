using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options; // Nécessaire pour IOptions
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CabinetMedicalWeb.Services
{
    // CORRECTION 1 : Il faut hériter de IEmailService
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<SmtpEmailService> _logger;

        // CORRECTION 2 : Utiliser IOptions (avec un s)
        public SmtpEmailService(IOptions<SmtpSettings> settings, ILogger<SmtpEmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            if (!_settings.Enabled)
            {
                _logger.LogInformation("Envoi d'e-mail ignoré car la configuration SMTP est désactivée.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.From))
            {
                _logger.LogWarning("Configuration SMTP invalide : hôte ou adresse expéditeur manquants.");
                return;
            }

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

            try
            {
                await client.SendMailAsync(message);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de l'email via SMTP {Host}:{Port}", _settings.Host, _settings.Port);
                throw;
            }
        }
    }
}