using System;
using System.Threading.Tasks;
using CabinetMedicalWeb.Models;
using Microsoft.Extensions.Logging;

namespace CabinetMedicalWeb.Services
{
    public class ReservationNotificationService : IReservationNotificationService
    {
        private readonly IEmailService _emailService;
        private readonly IViewRenderService _viewRenderService;
        private readonly ILogger<ReservationNotificationService> _logger;

        public ReservationNotificationService(
            IEmailService emailService,
            IViewRenderService viewRenderService,
            ILogger<ReservationNotificationService> logger)
        {
            _emailService = emailService;
            _viewRenderService = viewRenderService;
            _logger = logger;
        }

        public async Task NotifyStatusChangeAsync(ReservationRequest reservation, string previousStatus, DateTime? previousConfirmedDate)
        {
            if (reservation == null)
            {
                throw new ArgumentNullException(nameof(reservation));
            }

            try
            {
                var model = new ReservationNotificationModel
                {
                    Reservation = reservation,
                    PreviousStatus = previousStatus,
                    PreviousConfirmedDate = previousConfirmedDate
                };

                var subject = $"Mise à jour de votre réservation ({reservation.Statut})";
                var body = await _viewRenderService.RenderToStringAsync("~/Views/Shared/Email/ReservationStatusUpdate.cshtml", model);

                await _emailService.SendEmailAsync(reservation.Email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de la notification pour la réservation {ReservationId}", reservation.Id);
                throw;
            }
        }
    }
}
