using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CabinetMedicalWeb.Models
{
    public abstract class TenantEntity
    {
        public Guid TenantId { get; set; }

        [ForeignKey(nameof(TenantId))]
        public virtual Tenant? Tenant { get; set; }
    }
}
