using System;
using System.Linq;
using System.Threading.Tasks;
using CabinetMedicalWeb.Areas.FrontDesk.Models;
using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models;
using CabinetMedicalWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CabinetMedicalWeb.Areas.FrontDesk.Controllers
{
    [Area("FrontDesk")]
    [Authorize(Roles = "Secretaire,Admin")]
    public class ReservationRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<ReservationRequestsController> _logger;

        public ReservationRequestsController(ApplicationDbContext context, IEmailService emailService, ILogger<ReservationRequestsController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? statut)
        {
            var query = _context.ReservationRequests
                .Include(r => r.Doctor)
                .OrderByDescending(r => r.Id)
                .AsQueryable();

            statut ??= ReservationStatus.Pending;

            if (!string.IsNullOrWhiteSpace(statut))
            {
                query = query.Where(r => r.Statut == statut);
            }

            ViewData["FiltreStatut"] = statut;
            var reservations = await query.ToListAsync();
            return View(reservations);
        }

        public async Task<IActionResult> Review(int id)
        {
            var reservation = await _context.ReservationRequests.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            var viewModel = new ReservationApprovalViewModel
            {
                ReservationId = reservation.Id,
                Nom = reservation.Nom,
                Prenom = reservation.Prenom,
                Adresse = reservation.Adresse,
                Telephone = reservation.Telephone,
                Email = reservation.Email,
                DateNaissance = reservation.DateNaissance,
                DateSouhaitee = reservation.DateSouhaitee,
                Motif = reservation.Motif,
                DoctorId = reservation.DoctorId,
                DateHeure = reservation.DateHeureConfirmee ?? reservation.DateSouhaitee,
                Doctors = await GetDoctorsAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(ReservationApprovalViewModel model)
        {
            if (model.DateHeure < DateTime.Now)
            {
                ModelState.AddModelError(nameof(model.DateHeure), "La date confirmée doit être dans le futur.");
            }

            var reservation = await _context.ReservationRequests.FindAsync(model.ReservationId);
            if (reservation == null)
            {
                return NotFound();
            }

            if (!string.Equals(reservation.Statut, ReservationStatus.Pending, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ReservationApprouvee"] = "Cette demande a déjà été traitée.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                model.Doctors = await GetDoctorsAsync();
                return View("Review", model);
            }

            var conflict = await _context.RendezVous
                .AnyAsync(r => r.DoctorId == model.DoctorId && r.DateHeure == model.DateHeure);

            if (conflict)
            {
                ModelState.AddModelError(nameof(model.DateHeure), "Ce créneau est déjà réservé pour ce médecin.");
                model.Doctors = await GetDoctorsAsync();
                return View("Review", model);
            }

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Email == reservation.Email);
            if (patient == null)
            {
                patient = new Patient
                {
                    Nom = reservation.Nom,
                    Prenom = reservation.Prenom,
                    Adresse = reservation.Adresse ?? string.Empty,
                    Telephone = reservation.Telephone,
                    Email = reservation.Email,
                    DateNaissance = reservation.DateNaissance,
                    AntecedentsMedicaux = null
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
            }

            var rendezVous = new RendezVous
            {
                PatientId = patient.Id,
                DoctorId = model.DoctorId!,
                DateHeure = model.DateHeure,
                Motif = reservation.Motif,
                Statut = "Planifié"
            };

            _context.RendezVous.Add(rendezVous);

            reservation.Statut = ReservationStatus.Confirmed;
            reservation.PatientId = patient.Id;
            reservation.DoctorId = model.DoctorId;
            reservation.DateHeureConfirmee = model.DateHeure;

            await _context.SaveChangesAsync();

            // Send Confirmation Email
            if (!string.IsNullOrWhiteSpace(reservation.Email))
            {
                try
                {
                    var doctor = await _context.Users.FindAsync(model.DoctorId);
                    var doctorName = doctor != null ? $"Dr. {doctor.Nom}" : "un médecin du cabinet";
                    
                    var subject = "Confirmation de votre rendez-vous - Cabinet Médical Aurora";
                    var body = $@"
                        <div style='font-family: Arial, sans-serif; color: #333;'>
                            <h2 style='color: #10B981;'>Rendez-vous Confirmé</h2>
                            <p>Bonjour {reservation.Prenom} {reservation.Nom},</p>
                            <p>Nous avons le plaisir de confirmer votre demande de rendez-vous.</p>
                            <div style='background-color: #f9f9f9; padding: 15px; border-radius: 8px; border-left: 4px solid #10B981; margin: 15px 0;'>
                                <p><strong>Date et Heure :</strong> {model.DateHeure:dddd dd MMMM yyyy à HH:mm}</p>
                                <p><strong>Médecin :</strong> {doctorName}</p>
                                <p><strong>Motif :</strong> {reservation.Motif}</p>
                            </div>
                            <p>Merci de vous présenter 10 minutes avant l'heure du rendez-vous.</p>
                            <p>Cordialement,<br>L'équipe du Cabinet Médical Aurora</p>
                        </div>";

                    await _emailService.SendEmailAsync(reservation.Email, subject, body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de l'envoi de l'email de confirmation pour la réservation {Id}", reservation.Id);
                }
            }

            TempData["ReservationApprouvee"] = "La demande a été validée, le rendez-vous créé et l'email de confirmation envoyé.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refuser(int id)
        {
            var reservation = await _context.ReservationRequests.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            if (!string.Equals(reservation.Statut, ReservationStatus.Pending, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ReservationRefusee"] = "Cette demande a déjà été traitée.";
                return RedirectToAction(nameof(Index));
            }

            reservation.Statut = ReservationStatus.Rejected;
            await _context.SaveChangesAsync();

            // Send Rejection Email
            if (!string.IsNullOrWhiteSpace(reservation.Email))
            {
                try
                {
                    var subject = "Concernant votre demande de rendez-vous - Cabinet Médical Aurora";
                    var body = $@"
                        <div style='font-family: Arial, sans-serif; color: #333;'>
                            <h2 style='color: #EF4444;'>Demande de Rendez-vous</h2>
                            <p>Bonjour {reservation.Prenom} {reservation.Nom},</p>
                            <p>Nous ne sommes malheureusement pas en mesure de donner une suite favorable à votre demande de rendez-vous pour le motif suivant :</p>
                            <div style='background-color: #fff5f5; padding: 15px; border-radius: 8px; border-left: 4px solid #EF4444; margin: 15px 0;'>
                                <p>Le créneau demandé n'est plus disponible ou le médecin n'est pas disponible.</p>
                            </div>
                            <p>Nous vous invitons à nous contacter par téléphone ou à effectuer une nouvelle demande pour une autre date.</p>
                            <p>Cordialement,<br>L'équipe du Cabinet Médical Aurora</p>
                        </div>";

                    await _emailService.SendEmailAsync(reservation.Email, subject, body);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de l'envoi de l'email de refus pour la réservation {Id}", reservation.Id);
                }
            }

            TempData["ReservationRefusee"] = "La demande a été refusée et l'email de notification envoyé.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<SelectList> GetDoctorsAsync()
        {
            var medecinsData = await _context.Users
                .Where(u => !string.IsNullOrEmpty(u.Specialite))
                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                .Join(_context.Roles, combined => combined.ur.RoleId, r => r.Id, (combined, r) => new { combined.u, RoleName = r.Name })
                .Where(x => x.RoleName == "Medecin")
                .Select(x => new { x.u.Id, x.u.Prenom, x.u.Nom, x.u.Specialite })
                .OrderBy(x => x.Nom)
                .ToListAsync();

            var medecins = medecinsData
                .Select(m => new SelectListItem
                {
                    Value = m.Id,
                    Text = $"Dr. {m.Prenom} {m.Nom} ({m.Specialite ?? "Généraliste"})"
                })
                .ToList();

            return new SelectList(medecins, "Value", "Text");
        }
    }
}