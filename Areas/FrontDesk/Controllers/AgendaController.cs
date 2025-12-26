using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CabinetMedicalWeb.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CabinetMedicalWeb.Areas.FrontDesk.Controllers
{
    [Area("FrontDesk")]
    public class AgendaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AgendaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Calculer le début de la semaine (Lundi)
            var today = DateTime.Today;
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
            var startOfWeek = today.AddDays(-daysUntilMonday);

            // Calculer la fin de la semaine (Vendredi 23h59)
            var endOfWeek = startOfWeek.AddDays(4).AddHours(23).AddMinutes(59);

            // Récupérer tous les rendez-vous de la semaine avec les infos Patient et Doctor
            var rendezVous = await _context.RendezVous
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Where(r => r.DateHeure >= startOfWeek && r.DateHeure <= endOfWeek)
                .OrderBy(r => r.DateHeure)
                .ToListAsync();

            // Passer les dates pour affichage dans la vue
            ViewData["StartOfWeek"] = startOfWeek;
            ViewData["EndOfWeek"] = endOfWeek;

            return View(rendezVous);
        }
    }
}