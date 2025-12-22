using System;
using System.ComponentModel.DataAnnotations;

namespace CabinetMedicalWeb.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [StringLength(100, ErrorMessage = "Le prénom ne peut pas dépasser 100 caractères")]
        public string Prenom { get; set; } = string.Empty;

        [StringLength(200)]
        public string Adresse { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le téléphone est obligatoire")]
        [Phone(ErrorMessage = "Format de téléphone invalide")]
        [StringLength(20)]
        public string Telephone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La date de naissance est obligatoire")]
        [DataType(DataType.Date)]
        public DateTime DateNaissance { get; set; }

        public string? AntecedentsMedicaux { get; set; }

        // Navigation Property: Lien vers le Dossier Médical (One-to-One)
        // Un patient a un seul dossier médical
        public virtual DossierMedical? Dossier { get; set; }
    }
}