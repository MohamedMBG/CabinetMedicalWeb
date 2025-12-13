using Microsoft.AspNetCore.Mvc;

namespace CabinetMedicalWeb.Areas.FrontDesk.Controllers
{
    [Area("FrontDesk")] // Indispensable pour dire qu'on est dans la zone
    public class PlanningController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
