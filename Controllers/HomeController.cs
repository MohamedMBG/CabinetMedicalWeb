using System.Diagnostics;
using CabinetMedicalWeb.Models;
using CabinetMedicalWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
            // If user is logged in, check their role and redirect accordingly
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    
                    // Redirect doctors to Medical area
                    if (roles.Contains("Medecin"))
                    {
                        return RedirectToAction("Index", "DossierMedicals", new { area = "Medical" });
                    }
                    
                    // Redirect secretaries to FrontDesk area
                    if (roles.Contains("Secretaire"))
                    {
                        return RedirectToAction("Index", "Agenda", new { area = "FrontDesk" });
                    }
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
