using System.Diagnostics;
using CabinetMedicalWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
