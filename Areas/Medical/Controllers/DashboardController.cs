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
        public async Task<IActionResult> Index()
        {
            // 1. Identify the Doctor
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Index", "Home", new { area = "" });

            // 2. Calculate Date Ranges (Monday to Sunday)
            DateTime today = DateTime.Today;
            int currentDayOfWeek = (int)today.DayOfWeek; // 0 = Sunday
            // Adjust to make Monday (1) the start of the week. If Sunday(0), go back 6 days.
            DateTime startOfWeek = today.AddDays(currentDayOfWeek == 0 ? -6 : 1 - currentDayOfWeek);
            DateTime endOfWeek = startOfWeek.AddDays(7);

            // 3. Fetch Appointments (RendezVous)
            var appointments = await _context.RendezVous
                .Include(r => r.Patient)
                .Where(r => r.DoctorId == user.Id && 
                            r.DateHeure >= startOfWeek && 
                            r.DateHeure <= endOfWeek)
                .OrderBy(r => r.DateHeure)
                .ToListAsync();

            // 4. Prepare the ViewModel
            var model = new MedicalDashboardViewModel
            {
                DoctorProfile = user,
                WeekStartDate = startOfWeek,
                // Filter for "Today" section
                AppointmentsToday = appointments
                    .Where(r => r.DateHeure.Date == today)
                    .OrderBy(r => r.DateHeure)
                    .ToList(),
                // Initialize the calendar dictionary
                WeekCalendar = new Dictionary<DateTime, List<RendezVous>>()
            };

            // 5. Populate the Weekly Calendar (Day by Day)
            for (int i = 0; i < 7; i++)
            {
                var currentDay = startOfWeek.AddDays(i);
                model.WeekCalendar[currentDay] = appointments
                    .Where(r => r.DateHeure.Date == currentDay)
                    .ToList();
            }

            return View(model);
        }
    }
}