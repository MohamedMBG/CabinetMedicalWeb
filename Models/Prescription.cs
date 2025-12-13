using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CabinetMedicalWeb.Models
{
    public class Prescription
    {
        [Key]
        public int IdPrescription { get; set; } // Correspond à "idPrescription" du diagramme
        public DateTime DatePrescription { get; set; }
        public string Medicaments { get; set; } // Liste des médicaments

        // Lien vers le Dossier Médical
        public int DossierMedicalId { get; set; }
        [ForeignKey("DossierMedicalId")]
        public virtual DossierMedical DossierMedical { get; set; }

        // Lien vers le Médecin (celui qui prescrit)
        public string DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public virtual ApplicationUser Doctor { get; set; }
    }
}