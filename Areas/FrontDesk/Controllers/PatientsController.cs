using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models;
using CabinetMedicalWeb.Areas.FrontDesk.Models;
using CabinetMedicalWeb.Services;

namespace CabinetMedicalWeb.Areas.FrontDesk.Controllers
{
    [Area("FrontDesk")]
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentTenantService _currentTenantService;

        public PatientsController(ApplicationDbContext context, ICurrentTenantService currentTenantService)
        {
            _context = context;
            _currentTenantService = currentTenantService;
        }

        // GET: FrontDesk/Patients
        // Ajout du paramètre de recherche
        public async Task<IActionResult> Index(string searchString, int page = 1)
        {
            const int pageSize = 20;
            var patients = _context.Patients
                .AsNoTracking();

            // Filtrer si une recherche est effectuée
            if (!String.IsNullOrEmpty(searchString))
            {
                patients = patients.Where(p => p.Nom.Contains(searchString)
                                            || p.Prenom.Contains(searchString)
                                            || p.Telephone.Contains(searchString));
            }

            var totalCount = await patients.CountAsync();
            var results = await patients
                .OrderBy(p => p.Nom)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Passer la recherche à la vue pour la conserver dans le champ
            ViewData["CurrentFilter"] = searchString;
            ViewData["Page"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(results);
        }

        // GET: FrontDesk/Patients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .AsNoTracking()
                .Include(p => p.Dossier)
                    .ThenInclude(d => d.Consultations)
                .Include(p => p.Dossier)
                    .ThenInclude(d => d.Prescriptions)
                .Include(p => p.Dossier)
                    .ThenInclude(d => d.ResultatExamens)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null)
            {
                return NotFound();
            }

            var nextRendezVous = await _context.RendezVous
                .AsNoTracking()
                .Include(r => r.Doctor)
                .Where(r => r.PatientId == patient.Id && r.DateHeure >= DateTime.Now)
                .OrderBy(r => r.DateHeure)
                .FirstOrDefaultAsync();

            var viewModel = new PatientSnapshotViewModel
            {
                Patient = patient,
                Dossier = patient.Dossier,
                NextRendezVous = nextRendezVous
            };

            return View(viewModel);
        }

        // GET: FrontDesk/Patients/Create
        public IActionResult Create()
        {
            if (!_currentTenantService.HasTenant)
            {
                return NotFound();
            }

            return View();
        }

        // POST: FrontDesk/Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nom,Prenom,Adresse,Telephone,Email,DateNaissance,AntecedentsMedicaux")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        // GET: FrontDesk/Patients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }
            return View(patient);
        }

        // POST: FrontDesk/Patients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nom,Prenom,Adresse,Telephone,Email,DateNaissance,AntecedentsMedicaux")] Patient patient)
        {
            if (id != patient.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.Id))
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
            return View(patient);
        }

        // GET: FrontDesk/Patients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // POST: FrontDesk/Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(int id)
        {
            return _context.Patients.Any(e => e.Id == id);
        }
    }
}