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
            // Validate dossierId
            if (dossierId <= 0)
            {
                TempData["ErrorMessage"] = "Invalid medical record ID.";
                return RedirectToAction("Index", "DossierMedicals");
            }

            // Check if dossier exists
            var dossier = await _context.Dossiers
                .Include(d => d.Patient)
                .FirstOrDefaultAsync(d => d.Id == dossierId);

            if (dossier == null)
            {
                TempData["ErrorMessage"] = "Medical record not found.";
                return RedirectToAction("Index", "DossierMedicals");
            }

            // Create new consultation with default values
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
            // Validate DossierMedicalId
            if (consultation.DossierMedicalId <= 0)
            {
                ModelState.AddModelError("DossierMedicalId", "Medical record ID is required.");
            }
            else
            {
                // Verify dossier exists
                var dossier = await _context.Dossiers
                    .Include(d => d.Patient)
                    .FirstOrDefaultAsync(d => d.Id == consultation.DossierMedicalId);

                if (dossier == null)
                {
                    ModelState.AddModelError("DossierMedicalId", "Medical record not found.");
                }
                else
                {
                    ViewBag.PatientName = $"{dossier.Patient.Nom} {dossier.Patient.Prenom}";
                    ViewBag.DossierId = consultation.DossierMedicalId;
                }
            }

            // Ensure Notes is not null (database might require it)
            if (string.IsNullOrWhiteSpace(consultation.Notes))
            {
                consultation.Notes = string.Empty;
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(consultation.Motif))
            {
                ModelState.AddModelError("Motif", "Visit reason is required.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get current logged-in doctor
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser != null)
                    {
                        consultation.DoctorId = currentUser.Id;
                    }

                    _context.Add(consultation);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Consultation created successfully!";
                    return RedirectToAction("Details", "DossierMedicals", new { area = "Medical", id = consultation.DossierMedicalId });
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error creating consultation: {Message}", ex.Message);
                    ModelState.AddModelError("", $"An error occurred while creating the consultation: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error creating consultation");
                    ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                }
            }

            // If we get here, there was a validation error - reload patient info
            if (consultation.DossierMedicalId > 0)
            {
                var dossier = await _context.Dossiers
                    .Include(d => d.Patient)
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
