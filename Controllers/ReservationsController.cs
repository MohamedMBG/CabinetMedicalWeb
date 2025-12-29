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
        public async Task<IActionResult> Submit(ReservationRequest form)
        { 
            
            // Re-validate logic manually if needed or rely on Data Annotations
            if (form.DateSouhaitee < DateTime.Now)
            {
                ModelState.AddModelError("DateSouhaitee", "Veuillez choisir une date future pour la consultation.");
            }

            if (!ModelState.IsValid)
            {
                // If invalid, we need to reload the dashboard data (history) to redisplay the page correctly
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

            // Set default status
            form.Statut = ReservationStatus.Pending;
            
            _context.ReservationRequests.Add(form);
            await _context.SaveChangesAsync();

            TempData["ReservationSuccess"] = "Votre demande a été envoyée à la secrétaire pour validation.";
            
            // Redirect back to dashboard with email to show the new request in the list
            return RedirectToAction(nameof(Dashboard), new { email = form.Email });
        }
    }
}