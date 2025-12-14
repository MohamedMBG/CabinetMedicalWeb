using CabinetMedicalWeb.Areas.Medical.Models; // Indispensable pour le ViewModel
using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

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
        // Dans DossierMedicalsController.cs
        public IActionResult Create()
        {
            // C'est ce bloc qui est essentiel pour afficher Nom ET Prenom
            var patientsList = _context.Patients
                .Select(p => new {
                    Id = p.Id,
                    FullName = p.Nom + " " + p.Prenom // <--- DOIT CONTENIR CE SELECT
                })
                .ToList();

            ViewData["PatientId"] = new SelectList(patientsList, "Id", "FullName");
            return View();
        }

        // POST: Medical/DossierMedicals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PatientId")] DossierMedical dossierMedical)
        {
            // Vérification : Un patient ne doit pas avoir 2 dossiers
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

            // Si la validation échoue, on recharge la liste (avec le nom complet)
            var patientsList = _context.Patients
                .Select(p => new {
                    Id = p.Id,
                    FullName = p.Nom + " " + p.Prenom
                })
                .ToList();
            ViewData["PatientId"] = new SelectList(patientsList, "Id", "FullName", dossierMedical.PatientId);
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
            var patientsList = _context.Patients
                .Select(p => new {
                    Id = p.Id,
                    FullName = p.Nom + " " + p.Prenom
                })
                .ToList();
            ViewData["PatientId"] = new SelectList(patientsList, "Id", "FullName", dossierMedical.PatientId);
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
            var patientsList = _context.Patients
                .Select(p => new {
                    Id = p.Id,
                    FullName = p.Nom + " " + p.Prenom
                })
                .ToList();
            ViewData["PatientId"] = new SelectList(patientsList, "Id", "FullName", dossierMedical.PatientId);
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