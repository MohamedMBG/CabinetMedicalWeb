using System.ComponentModel.DataAnnotations;

namespace CabinetMedicalWeb.Models
{
    public class Patient
    {
        public int Id { get; set; }
        [Required]
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Adresse { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public DateTime DateNaissance { get; set; }
        public string? AntecedentsMedicaux { get; set; }

        // Liens
        public virtual DossierMedical? Dossier { get; set; }
    }
}