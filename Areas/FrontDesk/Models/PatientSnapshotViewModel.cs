using CabinetMedicalWeb.Models;

namespace CabinetMedicalWeb.Areas.FrontDesk.Models
{
    public class PatientSnapshotViewModel
    {
        public Patient Patient { get; set; } = null!;
        public DossierMedical? Dossier { get; set; }
        public RendezVous? NextRendezVous { get; set; }
    }
}
