using CabinetMedicalWeb.Areas.Medical.Models;
using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json.Serialization; // Pour le JSON (dans la nouvelle implémentation)

namespace CabinetMedicalWeb.Areas.Medical.Controllers
{
    [Area("Medical")]
    public class DossierMedicalsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DossierMedicalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // RECORD C# pour structurer les données envoyées à la vue/API
        // Ce record MUST correspond aux propriétés utilisées dans la vue JS/Razor
        private record PatientListItem(
            [property: JsonPropertyName("id")] int Id,
            [property: JsonPropertyName("fullName")] string FullName,
            [property: JsonPropertyName("telephone")] string Telephone,
            [property: JsonPropertyName("email")] string Email,
            [property: JsonPropertyName("dateNaissance")] string DateNaissance
        );


        // Helper pour construire la requête filtrée
        private IQueryable<Patient> BuildPatientsQuery(string? searchTerm = null)
        {
            var query = _context.Patients.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(p =>
                    EF.Functions.Like(p.Nom, $"%{searchTerm}%") ||
                    EF.Functions.Like(p.Prenom, $"%{searchTerm}%") ||
                    (!string.IsNullOrEmpty(p.Telephone) && EF.Functions.Like(p.Telephone, $"%{searchTerm}%")));
            }

            return query;
        }

        // Helper pour formater les données de sélection
        private async Task<List<PatientListItem>> GetPatientsForSelectionAsync(string? searchTerm = null)
        {
            return await BuildPatientsQuery(searchTerm)
                .OrderBy(p => p.Nom)
                .ThenBy(p => p.Prenom)
                .Select(p => new PatientListItem(
                    p.Id,
                    (p.Nom + " " + p.Prenom + " (" + p.DateNaissance.Year + ")").Trim(),
                    p.Telephone ?? string.Empty,
                    p.Email ?? string.Empty,
                    p.DateNaissance.ToString("yyyy-MM-dd") // Format ISO pour le JS
                ))
                .ToListAsync();
        }

        // Helper pour remplir le ViewData pour la Vue Create.cshtml
        private async Task PopulatePatientsDataAsync(int? selectedPatientId = null)
        {
            var patientsList = await GetPatientsForSelectionAsync();

            // 1. Pour la liste déroulante simple (fallback et POST)
            ViewData["PatientId"] = new SelectList(patientsList, "Id", "FullName", selectedPatientId);

            // 2. Pour le rendu initial de la vue (utilisé par @foreach dans Create.cshtml)
            // C'est ce ViewData qui contient toutes les métadonnées nécessaires pour les data-attributes HTML
            ViewData["InitialPatients"] = patientsList;
        }

        // Endpoint pour la recherche AJAX (utilisé par le script JS de Create.cshtml)
        [HttpGet]
        public async Task<IActionResult> PatientsList(string? searchTerm)
        {
            var patients = await GetPatientsForSelectionAsync(searchTerm);
            return Json(patients); // Retourne la liste des records PatientListItem directement
        }

        // GET: Medical/DossierMedicals
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Dossiers.Include(d => d.Patient);
            return View(await applicationDbContext.ToListAsync());
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
        public async Task<IActionResult> Create()
        {
            // Remplit ViewData["PatientId"] et ViewData["InitialPatients"]
            await PopulatePatientsDataAsync();
            return View();
        }

        // POST: Medical/DossierMedicals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PatientId")] DossierMedical dossierMedical)
        {
            if (_context.Dossiers.Any(d => d.PatientId == dossierMedical.PatientId))
            {
                ModelState.AddModelError("PatientId", "Ce patient a déjà un dossier médical.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(dossierMedical);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = dossierMedical.Id });
            }

            // En cas d'erreur, recharger les listes complètes
            await PopulatePatientsDataAsync(dossierMedical.PatientId);
            return View(dossierMedical);
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

            // Rechargement correct de la liste pour l'édition
            await PopulatePatientsDataAsync(dossierMedical.PatientId);
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

            // Rechargement correct de la liste si erreur de POST
            await PopulatePatientsDataAsync(dossierMedical.PatientId);
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