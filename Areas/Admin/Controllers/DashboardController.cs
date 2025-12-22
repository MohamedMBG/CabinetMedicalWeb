using System;
using System.Linq;
using System.Threading.Tasks;
using CabinetMedicalWeb.Areas.Admin.Models;
using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CabinetMedicalWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(-6);

            var doctorsInRole = await _userManager.GetUsersInRoleAsync("Medecin");

            var prescriptionStats = await _context.Prescriptions
                .Where(p => p.DatePrescription.Date >= startDate && p.DatePrescription.Date <= today)
                .GroupBy(p => p.DatePrescription.Date)
                .Select(g => new DailyPrescriptionStat
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                PatientsCount = await _context.Patients.CountAsync(),
                DoctorsCount = doctorsInRole.Count,
                MedicalFoldersCount = await _context.Dossiers.CountAsync(),
                PrescriptionsToday = await _context.Prescriptions.CountAsync(p => p.DatePrescription.Date == today),
                PrescriptionsByDay = Enumerable.Range(0, 7)
                    .Select(offset => startDate.AddDays(offset))
                    .Select(date => new DailyPrescriptionStat
                    {
                        Date = date,
                        Count = prescriptionStats.FirstOrDefault(p => p.Date == date)?.Count ?? 0
                    })
                    .ToList()
            };

            return View(viewModel);
        }
    }
}
