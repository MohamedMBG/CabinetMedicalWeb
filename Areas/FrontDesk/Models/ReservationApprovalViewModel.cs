using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CabinetMedicalWeb.Areas.FrontDesk.Models
{
    public class ReservationApprovalViewModel
    {
        public int ReservationId { get; set; }

        public string NomComplet => $"{Prenom} {Nom}";

        [Display(Name = "Nom")]
        public string Nom { get; set; } = string.Empty;

        [Display(Name = "Prénom")]
        public string Prenom { get; set; } = string.Empty;

        public string? Adresse { get; set; }
        public string Telephone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateNaissance { get; set; }
        public DateTime DateSouhaitee { get; set; }
        public string Motif { get; set; } = string.Empty;

        [Required(ErrorMessage = "Merci de sélectionner un médecin")] 
        [Display(Name = "Médecin attribué")]
        public string? DoctorId { get; set; }

        [Required(ErrorMessage = "Veuillez confirmer une date et heure")] 
        [Display(Name = "Date et heure confirmée")]
        [DataType(DataType.DateTime)]
        public DateTime DateHeure { get; set; }

        public IEnumerable<SelectListItem> Doctors { get; set; } = new List<SelectListItem>();
    }
}
