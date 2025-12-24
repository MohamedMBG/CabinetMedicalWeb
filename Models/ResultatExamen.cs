using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CabinetMedicalWeb.Models
{
    public class ResultatExamen
    {
        [Key]
        public int IdResultat { get; set; }
        public DateTime DateExamen { get; set; }
        public string TypeExamen { get; set; } // ex: "Sang", "Radio"
        public string Resultat { get; set; }

        // =========================== CLOUDINARY FIELDS ===========================
        [Display(Name = "Scan / Image")]
        public string? ScanUrl { get; set; } // The full URL to display
        
        public string? ScanPublicId { get; set; } // The Cloudinary ID (useful for deleting/replacing)
        
        // =========================== CLOUDINARY FIELDS ===========================

        // Lien vers le Dossier
        public int DossierMedicalId { get; set; }
        [ForeignKey("DossierMedicalId")]
        public virtual DossierMedical DossierMedical { get; set; }
    }
}