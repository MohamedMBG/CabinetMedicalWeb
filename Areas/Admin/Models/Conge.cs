using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CabinetMedicalWeb.Models
{
    public class Conge
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Date de début")]
        [DataType(DataType.Date)]
        public DateTime DateDebut { get; set; }

        [Required]
        [Display(Name = "Date de fin")]
        [DataType(DataType.Date)]
        public DateTime DateFin { get; set; }

        [Display(Name = "Motif (Optionnel)")]
        public string? Motif { get; set; } // Ex: Vacances, Maladie, Formation

        [Display(Name = "Statut")]
        public CongeStatus Status { get; set; } = CongeStatus.Pending;

        // Lien avec l'employé (Médecin ou Secrétaire)
        [Required]
        [Display(Name = "Employé")]
        public string PersonnelId { get; set; }

        [ForeignKey("PersonnelId")]
        public virtual ApplicationUser Personnel { get; set; }
    }
    public enum CongeStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}