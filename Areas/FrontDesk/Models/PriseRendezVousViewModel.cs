using CabinetMedicalWeb.Models;
namespace CabinetMedicalWeb.Areas.FrontDesk.Models
{
    public class PriseRendezVousViewModel
    {
        public int PatientId { get; set; }
        public string DoctorId { get; set; }
        public DateTime DateSouhaitee { get; set; }
        // Liste des médecins pour la liste déroulante
        public List<ApplicationUser> MedecinsDisponibles { get; set; }
    }
}