using System;
using System.ComponentModel.DataAnnotations;

namespace CabinetMedicalWeb.Models
{
    public static class ReservationStatus
    {
        public const string Pending = "PENDING";
        public const string Confirmed = "CONFIRMED";
        public const string Rejected = "REJECTED";
    }

    public class ReservationRequest : TenantEntity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100)]
        [Display(Name = "Last name")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100)]
        [Display(Name = "First name")]
        public string Prenom { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Address")]
        public string? Adresse { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        [StringLength(20)]
        [Display(Name = "Phone")]
        public string Telephone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [StringLength(150)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of birth")]
        public DateTime DateNaissance { get; set; }

        [Required(ErrorMessage = "Preferred date is required")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Preferred date & time")]
        public DateTime DateSouhaitee { get; set; }

        [Required(ErrorMessage = "Please tell us the reason for your visit")]
        [StringLength(500)]
        [Display(Name = "Reason for visit")]
        public string Motif { get; set; } = string.Empty;

        [StringLength(50)]
        public string Statut { get; set; } = ReservationStatus.Pending;

        public int? PatientId { get; set; }
        public virtual Patient? Patient { get; set; }

        public string? DoctorId { get; set; }
        public virtual ApplicationUser? Doctor { get; set; }

        public DateTime? DateHeureConfirmee { get; set; }
    }
}
