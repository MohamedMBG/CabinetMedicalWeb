using CabinetMedicalWeb.Models;
using CabinetMedicalWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity; // Required for UserManager
using System.Diagnostics;
using System.Threading.Tasks;

namespace CabinetMedicalWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1. Automatic Redirection based on Role
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                {
                    // Admins go to their Admin Dashboard
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                
                if (User.IsInRole("Medecin"))
                {
                    // UPDATE: Doctors now go to their specific Medical Dashboard (Calendar/Today)
                    // Instead of the dossier list
                    return RedirectToAction("Index", "Dashboard", new { area = "Medical" });
                }

                if (User.IsInRole("Secretaire"))
                {
                    // Secretaries go to the FrontDesk Agenda
                    return RedirectToAction("Index", "RendezVous", new { area = "FrontDesk" });
                }
            }

            // Public homepage for non-authenticated users
            var model = new ReservationRequest
            {
                DateSouhaitee = DateTime.Today.AddDays(1),
                DateNaissance = DateTime.Today.AddYears(-18)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReservation([Bind("Nom,Prenom,Adresse,Telephone,Email,DateNaissance,DateSouhaitee,Motif")] ReservationRequest form)
        {
            if (form.DateSouhaitee < DateTime.Now)
            {
                ModelState.AddModelError(nameof(form.DateSouhaitee), "Veuillez choisir une date future pour la consultation.");
            }

            if (!ModelState.IsValid)
            {
                return View("Index", form);
            }

            form.Statut = ReservationStatus.Pending;
            _context.ReservationRequests.Add(form);
            await _context.SaveChangesAsync();

            TempData["ReservationSuccess"] = "Votre demande a été envoyée à la secrétaire pour validation.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}