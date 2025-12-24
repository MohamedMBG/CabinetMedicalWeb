using CabinetMedicalWeb.Models;
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

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

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

            // If not logged in (or unknown role), show the public landing page
            return View();
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