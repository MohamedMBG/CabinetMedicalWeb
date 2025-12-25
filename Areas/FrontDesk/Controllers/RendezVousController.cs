using System;
using System.Linq;
using System.Threading.Tasks;
using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CabinetMedicalWeb.Areas.FrontDesk.Controllers
{
    [Area("FrontDesk")]
    public class RendezVousController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RendezVousController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FrontDesk/RendezVous/Create
        public IActionResult Create()
        {
            var model = new RendezVous
            {
                DateHeure = DateTime.Now.AddHours(1),
                Statut = "Planifié"
            };

            ChargerListes();
            return View(model);
        }

        // POST: FrontDesk/RendezVous/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DateHeure,Motif,PatientId,DoctorId,Statut")] RendezVous rendezVous)
        {
            if (!ModelState.IsValid)
            {
                ChargerListes();
                return View(rendezVous);
            }

            var conflit = await _context.RendezVous
                .AnyAsync(r => r.DoctorId == rendezVous.DoctorId && r.DateHeure == rendezVous.DateHeure);

            if (conflit)
            {
                ModelState.AddModelError(string.Empty, "Ce médecin a déjà un rendez-vous à cette date et heure.");
                ChargerListes();
                return View(rendezVous);
            }

            _context.RendezVous.Add(rendezVous);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Rendez-vous créé avec succès !";
            return RedirectToAction("Index", "Agenda", new { area = "FrontDesk" });
        }

        private void ChargerListes()
        {
            ViewData["DoctorId"] = _context.Users
                .Where(u => u.Role == "Doctor" || !string.IsNullOrEmpty(u.Specialite))
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"Dr. {u.Prenom} {u.Nom} ({u.Specialite ?? "Généraliste"})"
                })
                .OrderBy(x => x.Text)
                .ToList();

            ViewData["PatientId"] = _context.Patients
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Prenom} {p.Nom}"
                })
                .OrderBy(x => x.Text)
                .ToList();
        }
    }
}
