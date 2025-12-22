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
    public class PrescriptionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<PrescriptionController> _logger;

        public PrescriptionController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            ILogger<PrescriptionController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // =========================================================================================
        // 1. GET: FORMULAIRE DE CRÉATION
        // =========================================================================================
        // Initialise une nouvelle prescription liée à un dossier médical spécifique.
        // =========================================================================================
        [HttpGet]
        public async Task<IActionResult> Create(int dossierId)
        {
            if (dossierId <= 0)
            {
                TempData["ErrorMessage"] = "ID de dossier invalide.";
                return RedirectToAction("Index", "DossierMedicals");
            }

            // Vérification que le dossier existe bien
            var dossierExists = await _context.Dossiers.AnyAsync(d => d.Id == dossierId);
            if (!dossierExists)
            {
                TempData["ErrorMessage"] = "Le dossier médical spécifié est introuvable.";
                return RedirectToAction("Index", "DossierMedicals");
            }

            var prescription = new Prescription
            {
                DossierMedicalId = dossierId,
                DatePrescription = DateTime.Now // Date du jour par défaut
            };
            return View(prescription);
        }

        // =========================================================================================
        // 2. POST: ENREGISTREMENT EN BASE DE DONNÉES
        // =========================================================================================
        // Reçoit les données du formulaire, valide, assigne le médecin et sauvegarde.
        // =========================================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Prescription prescription)
        {
            // CRITIQUE : On retire les propriétés de navigation de la validation.
            // Le formulaire n'envoie que les IDs, donc les objets "DossierMedical" et "Doctor" sont null.
            // Sans cela, ModelState.IsValid renverrait toujours false.
            ModelState.Remove("DossierMedical");
            ModelState.Remove("Doctor");
            ModelState.Remove("DoctorId"); // Sera défini programmatiquement

            // Validation manuelle de l'existence du dossier
            var dossierExists = await _context.Dossiers.AnyAsync(d => d.Id == prescription.DossierMedicalId);
            if (!dossierExists)
            {
                ModelState.AddModelError("DossierMedicalId", "Le dossier médical associé n'existe pas.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Récupérer l'utilisateur connecté (le médecin)
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        prescription.DoctorId = user.Id;
                    }
                    else
                    {
                        // Cas rare : session perdue ou utilisateur supprimé
                        _logger.LogWarning("Tentative de création de prescription sans utilisateur connecté identifié.");
                    }

                    // 2. Sauvegarde en BDD
                    _context.Add(prescription);
                    await _context.SaveChangesAsync();

                    // 3. Feedback et Redirection
                    TempData["SuccessMessage"] = "Ordonnance enregistrée avec succès.";
                    return RedirectToAction("Details", "DossierMedicals", new { id = prescription.DossierMedicalId });
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Erreur base de données lors de la création de prescription.");
                    ModelState.AddModelError("", "Une erreur technique est survenue lors de l'enregistrement.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur inattendue dans Prescription/Create");
                    ModelState.AddModelError("", "Une erreur inattendue est survenue.");
                }
            }

            // Si on arrive ici, il y a une erreur -> Réafficher le formulaire avec les messages
            return View(prescription);
        }

        // =========================================================================================
        // 3. GET: VUE D'IMPRESSION (PDF)
        // =========================================================================================
        // Affiche une vue épurée spécialement conçue pour l'impression papier (A4).
        // =========================================================================================
        [HttpGet]
        public async Task<IActionResult> Print(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            // Récupération complète avec toutes les jointures nécessaires pour l'affichage
            // (Nom du patient, Nom du médecin, Date de naissance, etc.)
            var prescription = await _context.Prescriptions
                .Include(p => p.DossierMedical)
                    .ThenInclude(d => d.Patient)
                .Include(p => p.Doctor)
                // Utilisation de IdPrescription conformément à votre modèle
                .FirstOrDefaultAsync(m => m.IdPrescription == id);

            if (prescription == null)
            {
                return NotFound();
            }

            return View(prescription);
        }
    }
}