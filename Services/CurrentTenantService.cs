using System;
using CabinetMedicalWeb.Models;

namespace CabinetMedicalWeb.Services
{
    public class CurrentTenantService : ICurrentTenantService
    {
        public Tenant? Tenant { get; private set; }

        public Guid? TenantId => Tenant?.Id;

        public bool HasTenant => Tenant != null;

        public void SetTenant(Tenant tenant)
        {
            Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
        }
    }
}
