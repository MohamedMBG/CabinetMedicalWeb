using System;
using System.ComponentModel.DataAnnotations;

namespace CabinetMedicalWeb.Models
{
    public class ReservationRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [StringLength(100)]
        public string Prenom { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Adresse { get; set; }

        [Required(ErrorMessage = "Le téléphone est obligatoire")]
        [Phone]
        [StringLength(20)]
        public string Telephone { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date de naissance est obligatoire")]
        [DataType(DataType.Date)]
        public DateTime DateNaissance { get; set; }

        [Required(ErrorMessage = "La date souhaitée est obligatoire")]
        [DataType(DataType.DateTime)]
        public DateTime DateSouhaitee { get; set; }

        [Required(ErrorMessage = "Merci de préciser le motif de la consultation")]
        [StringLength(500)]
        public string Motif { get; set; } = string.Empty;

        [StringLength(50)]
        public string Statut { get; set; } = "En attente";

        public int? PatientId { get; set; }
        public virtual Patient? Patient { get; set; }

        public string? DoctorId { get; set; }
        public virtual ApplicationUser? Doctor { get; set; }

        public DateTime? DateHeureConfirmee { get; set; }
    }
}
