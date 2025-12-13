using Microsoft.AspNetCore.Mvc;

namespace CabinetMedicalWeb.Areas.Medical.Controllers
{
    [Area("Medical")]
    public class PrescriptionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
