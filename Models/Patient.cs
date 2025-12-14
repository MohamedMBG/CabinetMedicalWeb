using System;
using System.ComponentModel.DataAnnotations;

namespace CabinetMedicalWeb.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        public string Nom { get; set; }

        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        public string Prenom { get; set; }

        public string Adresse { get; set; }

        [Required(ErrorMessage = "Le téléphone est obligatoire.")]
        [Phone]
        public string Telephone { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date de Naissance")]
        public DateTime DateNaissance { get; set; }

        [Display(Name = "Antécédents Médicaux")]
        public string? AntecedentsMedicaux { get; set; }

        // Navigation Property: Lien vers le Dossier Médical (One-to-One)
        // Un patient a un seul dossier médical
        public virtual DossierMedical? Dossier { get; set; }
    }
}