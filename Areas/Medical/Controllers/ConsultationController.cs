using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CabinetMedicalWeb.Areas.Medical.Controllers
{
    [Area("Medical")]
    [Authorize(Roles = "Medecin,Admin")]
    public class ConsultationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ConsultationController> _logger;

        public ConsultationController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            ILogger<ConsultationController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Medical/Consultation/Create
        public async Task<IActionResult> Create(int dossierId)
        {
            if (dossierId <= 0)
            {
                TempData["ErrorMessage"] = "ID dossier invalide.";
                return RedirectToAction("Index", "DossierMedicals");
            }

            var dossier = await _context.Dossiers
                .Include(d => d.Patient)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == dossierId);

            if (dossier == null)
            {
                TempData["ErrorMessage"] = "Dossier introuvable.";
                return RedirectToAction("Index", "DossierMedicals");
            }

            var consultation = new Consultation
            {
                DossierMedicalId = dossierId,
                Date = DateTime.Now
            };

            ViewBag.PatientName = $"{dossier.Patient.Nom} {dossier.Patient.Prenom}";
            ViewBag.DossierId = dossierId;

            return View(consultation);
        }

        // POST: Medical/Consultation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Consultation consultation)
        {
            // 1. Nettoyage des erreurs de validation connues
            ModelState.Remove("DossierMedical");
            ModelState.Remove("Doctor");
            ModelState.Remove("DoctorId"); 
            
            if (string.IsNullOrWhiteSpace(consultation.Notes))
            {
                consultation.Notes = string.Empty;
                ModelState.Remove("Notes");
            }

            // 2. Validation explicite
            if (consultation.DossierMedicalId <= 0) 
                ModelState.AddModelError("DossierMedicalId", "L'ID du dossier est requis.");
            
            if (string.IsNullOrWhiteSpace(consultation.Motif)) 
                ModelState.AddModelError("Motif", "Le motif est obligatoire.");

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser != null) consultation.DoctorId = currentUser.Id;

                    _context.Add(consultation);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Consultation enregistrée !";
                    return RedirectToAction("Details", "DossierMedicals", new { area = "Medical", id = consultation.DossierMedicalId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur sauvegarde consultation");
                    ModelState.AddModelError("", "Erreur lors de la sauvegarde en base.");
                }
            }
            else
            {
                // --- DEBUG : AFFICHER LES ERREURS DANS LA CONSOLE ---
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    _logger.LogError($"ERREUR DE VALIDATION : {error.ErrorMessage} / {error.Exception?.Message}");
                }
                // ----------------------------------------------------
            }

            // Rechargement des infos patient pour la vue
            if (consultation.DossierMedicalId > 0)
            {
                var dossier = await _context.Dossiers
                    .Include(d => d.Patient)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == consultation.DossierMedicalId);
                
                if (dossier != null)
                {
                    ViewBag.PatientName = $"{dossier.Patient.Nom} {dossier.Patient.Prenom}";
                    ViewBag.DossierId = consultation.DossierMedicalId;
                }
            }

            return View(consultation);
        }
    }
}