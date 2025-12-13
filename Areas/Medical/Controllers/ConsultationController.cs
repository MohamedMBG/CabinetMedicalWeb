using Microsoft.AspNetCore.Mvc;
namespace CabinetMedicalWeb.Areas.Medical.Controllers
{
    [Area("Medical")] // Indispensable
    public class ConsultationController : Controller
    {
        public IActionResult Create(int dossierId)
        {
            return View();
        }
    }
}