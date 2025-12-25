using CabinetMedicalWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;
using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models;

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

        // Ton action Index existe déjà (l'agenda visuel)

        // GET: /FrontDesk/Agenda/Create
        public IActionResult Create()
        {
            var model = new RendezVous();
            ChargerListes(model);
            return View(model);
        }

        // POST: /FrontDesk/Agenda/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RendezVous model)
        {
            if (ModelState.IsValid)
            {
                // Vérification conflit : même docteur + même date/heure
                var conflit = await _context.RendezVous
                    .AnyAsync(r => r.DoctorId == model.DoctorId && r.DateHeure == model.DateSouhaitee);

                if (conflit)
                {
                    ModelState.AddModelError("", "⚠ Ce médecin a déjà un rendez-vous à cette date et heure. Veuillez choisir un autre créneau.");
                }
                else
                {
                    var rdv = new RendezVous
                    {
                        DoctorId = model.DoctorId,
                        PatientId = model.PatientId,
                        DateHeure = model.DateHeure,
                        Motif = model.Motif
                    };

                    _context.RendezVous.Add(rdv);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Rendez-vous créé avec succès !";
                    return RedirectToAction("Index");
                }
            }

            // En cas d'erreur → on recharge les listes
            ChargerListes(model);
            return View(model);
        }

        private void ChargerListes(RendezVous model)
        {
            // Liste des médecins (ApplicationUser avec rôle Doctor ou Specialite non vide)
            model.DoctorsDisponibles = _context.Users
                .Where(u => u.Role == "Doctor" || !string.IsNullOrEmpty(u.Specialite))
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"Dr. {u.Prenom} {u.Nom} ({u.Specialite ?? "Généraliste"})"
                })
                .OrderBy(x => x.Text)
                .ToList();

            // Liste des patients
            model.PatientsDisponibles = _context.Users
                .Where(u => u.Role == "Patient")
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.Prenom} {u.Nom}"
                })
                .OrderBy(x => x.Text)
                .ToList();
        }

        // Autres actions (Index, etc.) restent inchangées
    }
}