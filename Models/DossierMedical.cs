using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CabinetMedicalWeb.Models
{
    public class DossierMedical
    {
        [Key, ForeignKey("Patient")]
        public int PatientId { get; set; }
        public virtual Patient Patient { get; set; }
        public virtual ICollection<Consultation> Consultations { get; set; }
    }

    public class Consultation
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; }
        public int DossierMedicalId { get; set; }
        public virtual DossierMedical DossierMedical { get; set; }
    }
}