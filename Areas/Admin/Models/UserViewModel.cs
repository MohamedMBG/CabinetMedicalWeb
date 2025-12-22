using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CabinetMedicalWeb.Areas.Admin.Models
{
    public class UserViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Email requis")]
        [EmailAddress]
        [Display(Name = "Email (Login)")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Nom requis")]
        public string Nom { get; set; }

        [Required(ErrorMessage = "Prénom requis")]
        public string Prenom { get; set; }

        [Required(ErrorMessage = "Veuillez sélectionner un rôle")]
        [Display(Name = "Rôle")]
        public string Role { get; set; } // "Medecin" or "Secretaire"

        [Display(Name = "Spécialité (Médecins uniquement)")]
        public string? Specialite { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe provisoire")]
        public string? Password { get; set; } // If empty, use default

        // Helper for the dropdown list
        public IEnumerable<SelectListItem>? RolesList { get; set; }
    }
}