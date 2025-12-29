using System;

namespace CabinetMedicalWeb.Models
{
    public class ReservationNotificationModel
    {
        public ReservationRequest Reservation { get; set; } = new ReservationRequest();
        public string PreviousStatus { get; set; } = string.Empty;
        public DateTime? PreviousConfirmedDate { get; set; }
    }
}
