using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CabinetMedicalWeb.Models
{
    public class DossierMedical : TenantEntity
    {
        [Key]
        public int Id { get; set; }

        // Lien vers le Patient (Un dossier appartient à un patient)
        [Required]
        public int PatientId { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }

        // --- PARTIE MEDICALE (DEV B) ---
        // Ces listes permettent de dire : "Ce dossier contient tout ça..."

        public virtual ICollection<Consultation> Consultations { get; set; }
        public virtual ICollection<Prescription> Prescriptions { get; set; }
        public virtual ICollection<ResultatExamen> ResultatExamens { get; set; }

        // Constructeur pour éviter les erreurs "NullReference" sur les listes
        public DossierMedical()
        {
            Consultations = new List<Consultation>();
            Prescriptions = new List<Prescription>();
            ResultatExamens = new List<ResultatExamen>();
        }
    }
}