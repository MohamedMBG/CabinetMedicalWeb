using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CabinetMedicalWeb.Models
{
    public class Horaire : TenantEntity
    {
        [Key]
        public int IdHoraire { get; set; }
        public string JoursTravail { get; set; } // "Lundi,Mardi"
        public string HeuresTravail { get; set; } // "09:00-17:00"

        // Lien vers le Médecin
        public string DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public virtual ApplicationUser Doctor { get; set; }
    }
}