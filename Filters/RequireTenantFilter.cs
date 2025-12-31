using System.Threading.Tasks;
using CabinetMedicalWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CabinetMedicalWeb.Filters
{
    public class RequireTenantFilter : IAsyncActionFilter
    {
        private readonly ICurrentTenantService _currentTenantService;

        public RequireTenantFilter(ICurrentTenantService currentTenantService)
        {
            _currentTenantService = currentTenantService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!_currentTenantService.HasTenant)
            {
                context.Result = new NotFoundResult();
                return;
            }

            await next();
        }
    }
}
