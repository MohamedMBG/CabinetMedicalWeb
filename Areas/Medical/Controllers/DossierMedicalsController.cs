using CabinetMedicalWeb.Areas.Medical.Models;
using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CabinetMedicalWeb.Areas.Medical.Controllers
{
    [Area("Medical")]
    [Authorize(Roles = "Medecin,Admin")]
    public class DossierMedicalsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DossierMedicalsController> _logger;

        public DossierMedicalsController(ApplicationDbContext context, ILogger<DossierMedicalsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Medical/DossierMedicals
        public async Task<IActionResult> Index(string searchTerm = null)
        {
            IQueryable<DossierMedical> query = _context.Dossiers
                .Include(d => d.Patient)
                .Include(d => d.Consultations)
                .OrderByDescending(d => d.Id);

            // Search functionality
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(d =>
                    d.Patient.Nom.Contains(searchTerm) ||
                    d.Patient.Prenom.Contains(searchTerm) ||
                    (d.Patient.Telephone != null && d.Patient.Telephone.Contains(searchTerm)) ||
                    (d.Patient.Email != null && d.Patient.Email.Contains(searchTerm))
                );
            }

            ViewBag.SearchTerm = searchTerm;
            return View(await query.ToListAsync());
        }

        // GET: Medical/DossierMedicals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dossier = await _context.Dossiers
                .Include(d => d.Patient)
                .Include(d => d.Consultations)
                .Include(d => d.Prescriptions)
                .Include(d => d.ResultatExamens)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dossier == null)
            {
                return NotFound();
            }

            var viewModel = new DossierCompletViewModel
            {
                DossierId = dossier.Id,
                PatientInfo = dossier.Patient,
                Consultations = dossier.Consultations.OrderByDescending(c => c.Date).ToList(),
                Prescriptions = dossier.Prescriptions.OrderByDescending(p => p.DatePrescription).ToList(),
                Examens = dossier.ResultatExamens.OrderByDescending(e => e.DateExamen).ToList()
            };

            return View(viewModel);
        }

        // GET: Medical/DossierMedicals/Create
        public async Task<IActionResult> Create(string searchTerm = null)
        {
            // Get all patients who don't have a medical record yet
            var patientsWithRecords = await _context.Dossiers
                .Select(d => d.PatientId)
                .ToListAsync();

            // Start with base query - patients without medical records
            IQueryable<Patient> availablePatientsQuery = _context.Patients
                .Where(p => !patientsWithRecords.Contains(p.Id));

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                availablePatientsQuery = availablePatientsQuery.Where(p =>
                    p.Nom.Contains(searchTerm) ||
                    p.Prenom.Contains(searchTerm) ||
                    (p.Telephone != null && p.Telephone.Contains(searchTerm)) ||
                    (p.Email != null && p.Email.Contains(searchTerm))
                );
            }

            // Apply ordering after filtering
            availablePatientsQuery = availablePatientsQuery
                .OrderBy(p => p.Nom)
                .ThenBy(p => p.Prenom);

            var availablePatients = await availablePatientsQuery
                .Select(p => new PatientListItem(
                    p.Id,
                    $"{p.Nom} {p.Prenom} (Born: {p.DateNaissance.Year})",
                    p.Telephone ?? string.Empty,
                    p.Email ?? string.Empty,
                    p.DateNaissance.ToString("yyyy-MM-dd")
                ))
                .ToListAsync();

            ViewBag.AvailablePatients = availablePatients;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.PatientCount = availablePatients.Count;

            return View();
        }

        // POST: Medical/DossierMedicals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int patientId)
        {
            // Validate patientId
            if (patientId <= 0)
            {
                TempData["ErrorMessage"] = "Please select a patient.";
                return RedirectToAction(nameof(Create));
            }

            // Check if patient exists
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Selected patient does not exist.";
                return RedirectToAction(nameof(Create));
            }

            // Check if patient already has a medical record
            var existingDossier = await _context.Dossiers
                .Include(d => d.Patient)
                .FirstOrDefaultAsync(d => d.PatientId == patientId);

            if (existingDossier != null)
            {
                TempData["ErrorMessage"] = $"Patient {patient.Nom} {patient.Prenom} already has a medical record.";
                TempData["ExistingRecordId"] = existingDossier.Id;
                return RedirectToAction(nameof(Create));
            }

            // Create new medical record
            try
            {
                var dossierMedical = new DossierMedical
                {
                    PatientId = patientId
                };

                _context.Add(dossierMedical);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Medical record created successfully for {patient.Nom} {patient.Prenom}!";
                return RedirectToAction(nameof(Details), new { id = dossierMedical.Id });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error creating medical record for patient {PatientId}", patientId);
                TempData["ErrorMessage"] = "An error occurred while creating the medical record. Please try again.";
                return RedirectToAction(nameof(Create));
            }
        }

        // GET: Medical/DossierMedicals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dossierMedical = await _context.Dossiers.FindAsync(id);
            if (dossierMedical == null)
            {
                return NotFound();
            }

            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Nom", dossierMedical.PatientId);
            return View(dossierMedical);
        }

        // POST: Medical/DossierMedicals/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PatientId")] DossierMedical dossierMedical)
        {
            if (id != dossierMedical.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dossierMedical);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DossierMedicalExists(dossierMedical.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["PatientId"] = new SelectList(_context.Patients, "Id", "Nom", dossierMedical.PatientId);
            return View(dossierMedical);
        }

        // GET: Medical/DossierMedicals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dossierMedical = await _context.Dossiers
                .Include(d => d.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dossierMedical == null)
            {
                return NotFound();
            }

            return View(dossierMedical);
        }

        // POST: Medical/DossierMedicals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dossierMedical = await _context.Dossiers.FindAsync(id);
            if (dossierMedical != null)
            {
                _context.Dossiers.Remove(dossierMedical);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DossierMedicalExists(int id)
        {
            return _context.Dossiers.Any(e => e.Id == id);
        }
    }
}
