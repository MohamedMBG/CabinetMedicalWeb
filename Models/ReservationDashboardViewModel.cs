using System.Collections.Generic;

namespace CabinetMedicalWeb.Models
{
    public class ReservationDashboardViewModel
    {
        public ReservationRequest Form { get; set; } = new ReservationRequest();
        public List<ReservationRequest> Reservations { get; set; } = new();
    }
}
