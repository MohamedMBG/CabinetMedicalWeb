using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CabinetMedicalWeb.Areas.Medical.Controllers
{
    [Area("Medical")]
    public class LaboController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LaboController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Medical/Labo/Create?dossierId=5
        public IActionResult Create(int dossierId)
        {
            if (dossierId == 0)
            {
                return RedirectToAction("Index", "DossierMedicals");
            }

            var resultat = new ResultatExamen
            {
                DossierMedicalId = dossierId,
                DateExamen = DateTime.Now
            };
            return View(resultat);
        }

        // POST: Medical/Labo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ResultatExamen resultatExamen)
        {
            // Vérification de l'existence du dossier
            var dossierExists = await _context.Dossiers.AnyAsync(d => d.Id == resultatExamen.DossierMedicalId);
            
            if (!dossierExists)
            {
                ModelState.AddModelError("", "Dossier médical introuvable.");
            }

            if (ModelState.IsValid && dossierExists)
            {
                _context.Add(resultatExamen);
                await _context.SaveChangesAsync();
                
                // Retour au Dashboard du patient
                return RedirectToAction("Details", "DossierMedicals", new { id = resultatExamen.DossierMedicalId });
            }
            
            return View(resultatExamen);
        }
    }
}