using CabinetMedicalWeb.Models;
namespace CabinetMedicalWeb.Areas.Medical.Models
{
    public class DossierCompletViewModel
    {
        // Infos du Patient
        public Patient PatientInfo { get; set; }

        // Tout l'historique
        public List<Consultation> HistoriqueConsultations { get; set; }
        public List<Prescription> HistoriquePrescriptions { get; set; }
        public List<ResultatExamen> HistoriqueExamens { get; set; }
    }
}