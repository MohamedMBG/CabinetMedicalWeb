using System;
using CabinetMedicalWeb.Models;

namespace CabinetMedicalWeb.Services
{
    public interface ICurrentTenantService
    {
        Tenant? Tenant { get; }
        Guid? TenantId { get; }
        bool HasTenant { get; }
        void SetTenant(Tenant tenant);
    }
}
