using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CabinetMedicalWeb.Areas.Medical.Controllers
{
    [Area("Medical")]
    public class PrescriptionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PrescriptionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Medical/Prescription/Create?dossierId=5
        // Affiche le formulaire de création de prescription
        public IActionResult Create(int dossierId)
        {
            if (dossierId == 0)
            {
                return RedirectToAction("Index", "DossierMedicals");
            }

            var prescription = new Prescription
            {
                DossierMedicalId = dossierId,
                DatePrescription = DateTime.Now
            };
            return View(prescription);
        }

        // POST: Medical/Prescription/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Prescription prescription)
        {
            // Vérifier si le Dossier existe avant d'enregistrer
            var dossierExists = _context.Dossiers.Any(d => d.Id == prescription.DossierMedicalId);
            if (!dossierExists)
            {
                ModelState.AddModelError("", "Dossier médical introuvable.");
            }

            // Assigner l'ID du médecin actuellement connecté
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                prescription.DoctorId = user.Id;
            }

            if (ModelState.IsValid && dossierExists)
            {
                _context.Add(prescription);
                await _context.SaveChangesAsync();

                // Rediriger vers le Dashboard du dossier pour voir la nouvelle ordonnance
                return RedirectToAction("Details", "DossierMedicals", new { id = prescription.DossierMedicalId });
            }

            return View(prescription);
        }
    }
}