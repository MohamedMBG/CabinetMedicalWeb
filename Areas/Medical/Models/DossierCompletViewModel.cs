using System.Collections.Generic;
using CabinetMedicalWeb.Models;

namespace CabinetMedicalWeb.Areas.Medical.Models
{
    public class DossierCompletViewModel
    {
        public int DossierId { get; set; }
        public Patient PatientInfo { get; set; }

        // C'est l'absence de ces 3 lignes qui cause l'erreur CS0117
        public List<Consultation> Consultations { get; set; }
        public List<Prescription> Prescriptions { get; set; }
        public List<ResultatExamen> Examens { get; set; }
    }
}