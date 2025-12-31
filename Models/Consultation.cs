using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CabinetMedicalWeb.Models
{
    public class Consultation : TenantEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        public string Motif { get; set; } // Ex: "Douleur au ventre"

        public string Notes { get; set; } // Diagnostic du médecin

        // Lien vers le Dossier Médical (EXISTANT)
        public int DossierMedicalId { get; set; }

        [ForeignKey("DossierMedicalId")]
        public virtual DossierMedical DossierMedical { get; set; }

        // --- PARTIE AJOUTÉE POUR CORRIGER L'ERREUR ---
        // Lien vers le Médecin qui a fait la consultation
        public string? DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public virtual ApplicationUser? Doctor { get; set; }
    }
}