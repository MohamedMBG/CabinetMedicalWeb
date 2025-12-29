using System.Threading.Tasks;
using CabinetMedicalWeb.Models;

namespace CabinetMedicalWeb.Services
{
    public interface IReservationNotificationService
    {
        Task NotifyStatusChangeAsync(ReservationRequest reservation, string previousStatus, System.DateTime? previousConfirmedDate);
    }
}
