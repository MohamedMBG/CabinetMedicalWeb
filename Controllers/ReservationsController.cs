using System;
using System.Linq;
using System.Threading.Tasks;
using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CabinetMedicalWeb.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard(string? email)
        {
            var model = new ReservationDashboardViewModel
            {
                Form = new ReservationRequest
                {
                    DateSouhaitee = DateTime.Today.AddDays(1),
                    DateNaissance = DateTime.Today.AddYears(-18),
                    Email = email ?? string.Empty
                }
            };

            if (!string.IsNullOrWhiteSpace(email))
            {
                model.Reservations = await _context.ReservationRequests
                    .Include(r => r.Doctor)
                    .Where(r => r.Email == email)
                    .OrderByDescending(r => r.DateSouhaitee)
                    .ToListAsync();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit([Bind(Prefix = "Form", Include = "Nom,Prenom,Adresse,Telephone,Email,DateNaissance,DateSouhaitee,Motif")] ReservationRequest form)
        {
            if (form.DateSouhaitee < DateTime.Now)
            {
                ModelState.AddModelError(nameof(form.DateSouhaitee), "Veuillez choisir une date future pour la consultation.");
            }

            if (!ModelState.IsValid)
            {
                var invalidModel = new ReservationDashboardViewModel
                {
                    Form = form
                };

                if (!string.IsNullOrWhiteSpace(form.Email))
                {
                    invalidModel.Reservations = await _context.ReservationRequests
                        .Include(r => r.Doctor)
                        .Where(r => r.Email == form.Email)
                        .OrderByDescending(r => r.DateSouhaitee)
                        .ToListAsync();
                }

                return View("Dashboard", invalidModel);
            }

            form.Statut = ReservationStatus.Pending;
            _context.ReservationRequests.Add(form);
            await _context.SaveChangesAsync();

            TempData["ReservationSuccess"] = "Votre demande a été envoyée à la secrétaire pour validation.";
            return RedirectToAction(nameof(Dashboard), new { email = form.Email });
        }
    }
}
