using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CabinetMedicalWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CongesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CongesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Conges
        public async Task<IActionResult> Index()
        {
            var conges = await _context.Conges
                .Include(c => c.Personnel)
                .OrderByDescending(c => c.DateDebut)
                .ToListAsync();
            return View(conges);
        }

        // GET: Admin/Conges/Create
        public async Task<IActionResult> Create()
        {
            // Populate the dropdown with employees (Doctors and Secretaries)
            var employees = await _userManager.Users
                .Where(u => !string.IsNullOrEmpty(u.Nom)) // Filter out incomplete users if any
                .Select(u => new 
                { 
                    Id = u.Id, 
                    FullName = u.Nom + " " + u.Prenom 
                })
                .ToListAsync();

            ViewData["PersonnelId"] = new SelectList(employees, "Id", "FullName");
            return View();
        }

        // POST: Admin/Conges/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DateDebut,DateFin,Motif,PersonnelId")] Conge conge)
        {
            // --- FIX START ---
            // Remove the navigation property from validation because it's null during submission
            ModelState.Remove("Personnel");
            // --- FIX END ---

            if (conge.DateFin < conge.DateDebut)
            {
                ModelState.AddModelError("DateFin", "La date de fin ne peut pas être avant la date de début.");
            }

            if (ModelState.IsValid)
            {
                conge.IsApproved = true;
                _context.Add(conge);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Reload dropdown if validation fails
            var employees = await _userManager.Users
               .Select(u => new { Id = u.Id, FullName = u.Nom + " " + u.Prenom })
               .ToListAsync();
            ViewData["PersonnelId"] = new SelectList(employees, "Id", "FullName", conge.PersonnelId);
            
            return View(conge);
        }

        // GET: Admin/Conges/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var conge = await _context.Conges
                .Include(c => c.Personnel)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (conge == null) return NotFound();

            return View(conge);
        }

        // POST: Admin/Conges/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var conge = await _context.Conges.FindAsync(id);
            if (conge != null)
            {
                _context.Conges.Remove(conge);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var conge = await _context.Conges.FindAsync(id);
            if (conge == null) return NotFound();

            conge.IsApproved = true;
            _context.Update(conge);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}