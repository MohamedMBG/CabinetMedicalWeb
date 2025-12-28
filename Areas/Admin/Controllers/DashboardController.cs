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

        private static DateTime GetStartOfWeek(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(-6);

            var doctorsInRole = await _userManager.GetUsersInRoleAsync("Medecin");

            var doctorAppointments = await _context.RendezVous
                .Where(r => r.DateHeure.Date >= today.AddDays(-30))
                .GroupBy(r => r.DoctorId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            var doctorPrescriptions = await _context.Prescriptions
                .Where(p => p.DatePrescription.Date >= today.AddDays(-30))
                .GroupBy(p => p.DoctorId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            var weeklyStart = GetStartOfWeek(today).AddDays(-7 * 5);
            var monthlyStart = new DateTime(today.Year, today.Month, 1).AddMonths(-5);

            var weeklyAppointments = (await _context.RendezVous
                    .Where(r => r.DateHeure.Date >= weeklyStart && r.DateHeure.Date <= today)
                    .ToListAsync())
                .GroupBy(r => GetStartOfWeek(r.DateHeure.Date))
                .Select(g => new
                {
                    Week = g.Key,
                    PatientCount = g.Select(r => r.PatientId).Distinct().Count(),
                    ReservationCount = g.Count()
                })
                .ToDictionary(x => x.Week, x => x);

            var weeklyReservations = (await _context.ReservationRequests
                    .Where(r => r.DateSouhaitee.Date >= weeklyStart && r.DateSouhaitee.Date <= today)
                    .ToListAsync())
                .GroupBy(r => GetStartOfWeek(r.DateSouhaitee.Date))
                .Select(g => new { Week = g.Key, ReservationCount = g.Count() })
                .ToDictionary(x => x.Week, x => x.ReservationCount);

            var monthlyAppointments = await _context.RendezVous
                .Where(r => r.DateHeure.Date >= monthlyStart && r.DateHeure.Date <= today)
                .GroupBy(r => new { r.DateHeure.Year, r.DateHeure.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    PatientCount = g.Select(r => r.PatientId).Distinct().Count(),
                    ReservationCount = g.Count()
                })
                .ToDictionaryAsync(x => (x.Year, x.Month), x => x);

            var monthlyReservations = await _context.ReservationRequests
                .Where(r => r.DateSouhaitee.Date >= monthlyStart && r.DateSouhaitee.Date <= today)
                .GroupBy(r => new { r.DateSouhaitee.Year, r.DateSouhaitee.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, ReservationCount = g.Count() })
                .ToDictionaryAsync(x => (x.Year, x.Month), x => x.ReservationCount);

            var doctorPerformance = doctorsInRole
                .Select(d => new UserPerformanceStat
                {
                    Name = $"Dr. {d.Prenom} {d.Nom}",
                    Specialite = d.Specialite,
                    Appointments = doctorAppointments.TryGetValue(d.Id, out var appts) ? appts : 0,
                    Prescriptions = doctorPrescriptions.TryGetValue(d.Id, out var px) ? px : 0
                })
                .OrderByDescending(d => d.Appointments + d.Prescriptions)
                .ToList();

            var reservationPerformance = new ReservationWorkflowPerformance
            {
                Confirmed = await _context.ReservationRequests.CountAsync(r => r.Statut == ReservationStatus.Confirmed && r.DateSouhaitee.Date >= today.AddDays(-30)),
                Rejected = await _context.ReservationRequests.CountAsync(r => r.Statut == ReservationStatus.Rejected && r.DateSouhaitee.Date >= today.AddDays(-30)),
                Pending = await _context.ReservationRequests.CountAsync(r => r.Statut == ReservationStatus.Pending)
            };

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
                    .ToList(),
                WeeklyPatientReservations = Enumerable.Range(0, 6)
                    .Select(weekOffset => weeklyStart.AddDays(7 * weekOffset))
                    .Select(week => new PatientReservationPoint
                    {
                        Label = $"Semaine du {week:dd MMM}",
                        Patients = weeklyAppointments.TryGetValue(week, out var data) ? data.PatientCount : 0,
                        Reservations = weeklyReservations.TryGetValue(week, out var reservations) ? reservations : 0
                    })
                    .ToList(),
                MonthlyPatientReservations = Enumerable.Range(0, 6)
                    .Select(monthOffset => monthlyStart.AddMonths(monthOffset))
                    .Select(month => new PatientReservationPoint
                    {
                        Label = month.ToString("MMM yyyy"),
                        Patients = monthlyAppointments.TryGetValue((month.Year, month.Month), out var data) ? data.PatientCount : 0,
                        Reservations = monthlyReservations.TryGetValue((month.Year, month.Month), out var reservations) ? reservations : 0
                    })
                    .ToList(),
                DoctorPerformance = doctorPerformance,
                ReceptionPerformance = reservationPerformance
            };

            return View(viewModel);
        }
    }
}
