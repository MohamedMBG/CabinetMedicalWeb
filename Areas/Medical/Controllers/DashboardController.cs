using CabinetMedicalWeb.Areas.Medical.Models;
using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CabinetMedicalWeb.Areas.Medical.Controllers
{
    [Area("Medical")]
    [Authorize(Roles = "Medecin")] // Strictly for Doctors
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Medical/Dashboard
        public async Task<IActionResult> Index(DateTime? weekStart)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home", new { area = "" });

            var referenceDate = weekStart ?? DateTime.Today;
            var model = await BuildDashboardModel(user, referenceDate);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestConge(DateTime dateDebut, DateTime dateFin, string? motif)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home", new { area = "" });

            if (dateFin < dateDebut)
            {
                ModelState.AddModelError("CongeDate", "La date de fin ne peut pas être avant la date de début.");
            }

            if (!ModelState.IsValid)
            {
                var modelWithErrors = await BuildDashboardModel(user, dateDebut);
                modelWithErrors.NewCongeRequest = new Conge
                {
                    DateDebut = dateDebut,
                    DateFin = dateFin,
                    Motif = motif,
                    PersonnelId = user.Id
                };

                return View("Index", modelWithErrors);
            }

            var conge = new Conge
            {
                DateDebut = dateDebut.Date,
                DateFin = dateFin.Date,
                Motif = motif,
                PersonnelId = user.Id,
                Status = CongeStatus.Pending
            };

            _context.Conges.Add(conge);
            await _context.SaveChangesAsync();
            TempData["CongeSuccess"] = "Votre demande de congé a été envoyée à l'administrateur.";

            return RedirectToAction(nameof(Index), new { weekStart = GetStartOfWeek(dateDebut).ToString("yyyy-MM-dd") });
        }

        private async Task<MedicalDashboardViewModel> BuildDashboardModel(ApplicationUser user, DateTime referenceDate)
        {
            DateTime today = DateTime.Today;
            var startOfWeek = GetStartOfWeek(referenceDate);
            var endOfWeek = startOfWeek.AddDays(6);
            var startBoundary = startOfWeek;
            var endBoundary = endOfWeek.AddDays(1);

            var appointments = await _context.RendezVous
                .Include(r => r.Patient)
                .Where(r => r.DoctorId == user.Id &&
                            r.DateHeure >= startBoundary &&
                            r.DateHeure < endBoundary)
                .OrderBy(r => r.DateHeure)
                .ToListAsync();

            var weeklyConges = await _context.Conges
                .Where(c => c.PersonnelId == user.Id &&
                            c.Status == CongeStatus.Approved &&
                            c.DateFin >= startOfWeek &&
                            c.DateDebut <= endOfWeek)
                .OrderBy(c => c.DateDebut)
                .ToListAsync();

            var upcomingConges = await _context.Conges
                .Where(c => c.PersonnelId == user.Id && c.Status == CongeStatus.Approved && c.DateFin >= today)
                .OrderBy(c => c.DateDebut)
                .Take(3)
                .ToListAsync();

            var model = new MedicalDashboardViewModel
            {
                DoctorProfile = user,
                WeekStartDate = startOfWeek,
                WeekEndDate = endOfWeek,
                PreviousWeekStart = startOfWeek.AddDays(-7),
                NextWeekStart = startOfWeek.AddDays(7),
                AppointmentsToday = appointments
                    .Where(r => r.DateHeure.Date == today)
                    .OrderBy(r => r.DateHeure)
                    .ToList(),
                WeekCalendar = new Dictionary<DateTime, List<RendezVous>>(),
                WeeklyConges = weeklyConges,
                UpcomingConges = upcomingConges,
                NewCongeRequest = new Conge
                {
                    DateDebut = today,
                    DateFin = today.AddDays(1),
                    PersonnelId = user.Id
                }
            };

            for (int i = 0; i < 7; i++)
            {
                var currentDay = startOfWeek.AddDays(i);
                model.WeekCalendar[currentDay] = appointments
                    .Where(r => r.DateHeure.Date == currentDay)
                    .ToList();
            }

            return model;
        }

        private static DateTime GetStartOfWeek(DateTime date)
        {
            int currentDayOfWeek = (int)date.DayOfWeek; // 0 = Sunday
            return date.Date.AddDays(currentDayOfWeek == 0 ? -6 : 1 - currentDayOfWeek);
        }
    }
}