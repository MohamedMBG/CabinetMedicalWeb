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
    [Authorize(Roles = "Medecin,Admin")] // Restrict access to doctors and admins only
    public class DossierMedicalsController : Controller
    {
        // Database context for data operations
        private readonly ApplicationDbContext _context;
        
        // Logger for tracking application events and errors
        private readonly ILogger<DossierMedicalsController> _logger;

        /// <summary>
        /// Constructor with dependency injection for database context and logger
        /// </summary>
        public DossierMedicalsController(ApplicationDbContext context, ILogger<DossierMedicalsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Index - List Medical Records

        /// <summary>
        /// GET: Medical/DossierMedicals
        /// Displays all medical records with optional search functionality
        /// </summary>
        /// <param name="searchTerm">Optional search term to filter records</param>
        /// <returns>View with list of medical records</returns>
        public async Task<IActionResult> Index(string searchTerm = null)
        {
            // Base query: Include related Patient and Consultations data, ordered by newest first
            IQueryable<DossierMedical> query = _context.Dossiers
                .Include(d => d.Patient)
                .Include(d => d.Consultations)
                .OrderByDescending(d => d.Id);

            // Apply search filter if a search term is provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(d =>
                    d.Patient.Nom.Contains(searchTerm) ||
                    d.Patient.Prenom.Contains(searchTerm) ||
                    (d.Patient.Telephone != null && d.Patient.Telephone.Contains(searchTerm))
                );
            }

            // Pass search term back to view for display in search box
            ViewBag.SearchTerm = searchTerm;
            
            return View(await query.ToListAsync());
        }

        #endregion

        #region Details - View Complete Medical Record

        /// <summary>
        /// GET: Medical/DossierMedicals/Details/5
        /// Displays complete medical record including all related data
        /// </summary>
        /// <param name="id">Medical record ID</param>
        /// <returns>View with complete medical record details</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Load medical record with all related entities
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

            // Create view model to organize all related data
            // Note: Ensure DossierCompletViewModel is defined in your Models
            var viewModel = new DossierCompletViewModel
            {
                DossierId = dossier.Id,
                PatientInfo = dossier.Patient,
                // Order related entities by date (newest first)
                Consultations = dossier.Consultations.OrderByDescending(c => c.Date).ToList(),
                Prescriptions = dossier.Prescriptions.OrderByDescending(p => p.DatePrescription).ToList(),
                Examens = dossier.ResultatExamens.OrderByDescending(e => e.DateExamen).ToList()
            };

            return View(viewModel);
        }

        #endregion

        #region Create - New Medical Record

        /// <summary>
        /// GET: Medical/DossierMedicals/Create
        /// Displays form to create a new medical record for patients without one
        /// </summary>
        /// <param name="searchTerm">Optional search term to filter available patients</param>
        /// <returns>View with list of patients without medical records</returns>
        public async Task<IActionResult> Create(string searchTerm = null)
        {
            // 1. Get IDs of patients who ALREADY have records
            var patientsWithRecords = await _context.Dossiers
                .Select(d => d.PatientId)
                .ToListAsync();

            // 2. Filter patients excluding those IDs
            IQueryable<Patient> availablePatientsQuery = _context.Patients
                .Where(p => !patientsWithRecords.Contains(p.Id));

            // 3. Apply search
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

            // 4. Order and Project
            var availablePatients = await availablePatientsQuery
                .OrderBy(p => p.Nom)
                .ThenBy(p => p.Prenom)
                .Select(p => new PatientListItem(
                    p.Id,
                    $"{p.Nom} {p.Prenom} (Born: {p.DateNaissance.Year})",
                    p.Telephone ?? "",
                    p.Email ?? "",
                    p.DateNaissance.ToString("yyyy-MM-dd")
                ))
                .ToListAsync();

            ViewBag.AvailablePatients = availablePatients;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.PatientCount = availablePatients.Count;

            return View();
        }

        /// <summary>
        /// POST: Medical/DossierMedicals/Create
        /// Creates a new medical record for the selected patient
        /// </summary>
        /// <param name="patientId">ID of patient to create record for</param>
        /// <returns>Redirects to medical record details or back to form with error</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [FromForm] is CRITICAL here: ensures the int is bound correctly from the form body
        public async Task<IActionResult> Create([FromForm] int patientId)
        {
            // Validation: Check if patient ID is valid
            if (patientId <= 0)
            {
                TempData["ErrorMessage"] = "Please select a patient to create a record.";
                return RedirectToAction(nameof(Create));
            }

            // Validation: Check if patient exists
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                TempData["ErrorMessage"] = "Selected patient does not exist.";
                return RedirectToAction(nameof(Create));
            }

            // Double check validation to prevent duplicates
            var exists = await _context.Dossiers.AnyAsync(d => d.PatientId == patientId);
            if (exists)
            {
                TempData["ErrorMessage"] = "This patient already has a medical record.";
                return RedirectToAction(nameof(Create));
            }

            // Create new medical record
            try
            {
                var dossierMedical = new DossierMedical
                {
                    PatientId = patientId
                    // Add creation date if your model supports it, e.g., DateCreation = DateTime.Now
                };

                _context.Add(dossierMedical);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Medical record created for {patient.Nom} {patient.Prenom}!";
                return RedirectToAction(nameof(Details), new { id = dossierMedical.Id });
            }
            catch (DbUpdateException ex)
            {
                // Log the error for debugging
                _logger.LogError(ex, "Error creating dossier for patient {PatientId}", patientId);
                TempData["ErrorMessage"] = "Database error occurred. Please try again.";
                return RedirectToAction(nameof(Create));
            }
        }

        #endregion

        #region Edit - Update Patient Information

        /// <summary>
        /// GET: Medical/DossierMedicals/Edit/5
        /// Displays form to edit patient information for a medical record
        /// </summary>
        /// <param name="id">Medical record ID</param>
        /// <returns>View with patient edit form</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Load medical record with patient data
            var dossierMedical = await _context.Dossiers
                .Include(d => d.Patient)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dossierMedical == null || dossierMedical.Patient == null)
            {
                return NotFound();
            }

            ViewBag.DossierId = id; // Pass dossier ID to view
            return View(dossierMedical.Patient); // Edit the patient entity
        }

        /// <summary>
        /// POST: Medical/DossierMedicals/Edit/5
        /// Updates patient information for a medical record
        /// </summary>
        /// <param name="id">Patient ID</param>
        /// <param name="patient">Updated patient data</param>
        /// <returns>Redirects to details or index</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nom,Prenom,DateNaissance,Adresse,Telephone,Email,AntecedentsMedicaux")] Patient patient)
        {
            // Security check: Ensure URL ID matches form data ID
            if (id != patient.Id)
            {
                return NotFound();
            }

            // Validate model state (data annotations)
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Patient information updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrent modification
                    if (!_context.Patients.Any(p => p.Id == patient.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Redirect to the medical record details page
                var dossier = await _context.Dossiers.FirstOrDefaultAsync(d => d.PatientId == patient.Id);
                if (dossier != null)
                {
                    return RedirectToAction(nameof(Details), new { id = dossier.Id });
                }
                return RedirectToAction(nameof(Index));
            }
            
            // If validation fails, return to edit form with existing data
            return View(patient);
        }

        #endregion

        #region Delete - Remove Medical Record

        /// <summary>
        /// GET: Medical/DossierMedicals/Delete/5
        /// Displays confirmation page for deleting a medical record
        /// </summary>
        /// <param name="id">Medical record ID</param>
        /// <returns>View with delete confirmation</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Load medical record with patient data for confirmation display
            var dossierMedical = await _context.Dossiers
                .Include(d => d.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dossierMedical == null)
            {
                return NotFound();
            }

            return View(dossierMedical);
        }

        /// <summary>
        /// POST: Medical/DossierMedicals/Delete/5
        /// Deletes a medical record (confirmation action)
        /// </summary>
        /// <param name="id">Medical record ID</param>
        /// <returns>Redirects to index</returns> 

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks if a medical record exists
        /// </summary>
        /// <param name="id">Medical record ID</param>
        /// <returns>True if record exists, false otherwise</returns>
        private bool DossierMedicalExists(int id)
        {
            return _context.Dossiers.Any(e => e.Id == id);
        }
        
        #endregion

        // POST: Medical/DossierMedicals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dossierMedical = await _context.Dossiers.FindAsync(id);
            if (dossierMedical != null)
            {
                _context.Dossiers.Remove(dossierMedical);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ====================================================================
        // NOUVEAU : VUE COMPLETE POUR EXPORT PDF / IMPRESSION
        // ====================================================================
        public async Task<IActionResult> FullRecord(int? id)
        {
            if (id == null) return NotFound();

            var dossier = await _context.Dossiers
                .Include(d => d.Patient)
                .Include(d => d.Consultations).ThenInclude(c => c.Doctor)
                .Include(d => d.Prescriptions).ThenInclude(p => p.Doctor)
                .Include(d => d.ResultatExamens)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dossier == null) return NotFound();

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

    }

    
}