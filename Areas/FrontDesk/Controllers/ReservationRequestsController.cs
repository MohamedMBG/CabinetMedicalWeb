using System;
using System.Linq;
using System.Threading.Tasks;
using CabinetMedicalWeb.Areas.FrontDesk.Models;
using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CabinetMedicalWeb.Areas.FrontDesk.Controllers
{
    [Area("FrontDesk")]
    [Authorize(Roles = "Secretaire,Admin")]
    public class ReservationRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? statut)
        {
            var query = _context.ReservationRequests
                .Include(r => r.Doctor)
                .OrderByDescending(r => r.Id)
                .AsQueryable();

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

            reservation.Statut = "Approuvé";
            reservation.PatientId = patient.Id;
            reservation.DoctorId = model.DoctorId;
            reservation.DateHeureConfirmee = model.DateHeure;

            await _context.SaveChangesAsync();

            TempData["ReservationApprouvee"] = "La demande a été validée et le rendez-vous a été créé.";
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

            reservation.Statut = "Refusé";
            await _context.SaveChangesAsync();

            TempData["ReservationRefusee"] = "La demande a été refusée.";
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
