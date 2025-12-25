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
            var medecins = _context.Users
                .Where(u => !string.IsNullOrEmpty(u.Specialite))
                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                .Join(_context.Roles, combined => combined.ur.RoleId, r => r.Id, (combined, r) => new { combined.u, RoleName = r.Name })
                .Where(x => x.RoleName == "Medecin")
                .Select(x => new SelectListItem
                {
                    Value = x.u.Id,
                    Text = $"Dr. {x.u.Prenom} {x.u.Nom} ({x.u.Specialite ?? "Généraliste"})"
                })
                .OrderBy(x => x.Text)
                .ToList();

            ViewData["DoctorId"] = medecins;

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
