using System;
using System.ComponentModel.DataAnnotations;

namespace CabinetMedicalWeb.Models
{
    public class RendezVous : TenantEntity
    {
        public int Id { get; set; }
        public DateTime DateHeure { get; set; }
        public string Motif { get; set; }
        public string Statut { get; set; } // "Planifié", "Annulé"

        // Clés étrangères
        public int PatientId { get; set; }
        public virtual Patient Patient { get; set; }

        public string DoctorId { get; set; }
        public virtual ApplicationUser Doctor { get; set; }
    }
}