//namespace CabinetMedicalWeb.Models
using Microsoft.AspNetCore.Identity;

namespace CabinetMedicalWeb.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string? Specialite { get; set; } // "Cardiologue", "Dentiste" ou Null (pour secrétaire)

        public Guid TenantId { get; set; }
        public virtual Tenant? Tenant { get; set; }
    }
}