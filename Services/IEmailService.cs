using System.Threading.Tasks;

namespace CabinetMedicalWeb.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}