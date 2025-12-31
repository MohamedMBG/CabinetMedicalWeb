using System;
using System.Linq;
using System.Threading.Tasks;
using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models;
using CabinetMedicalWeb.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CabinetMedicalWeb.Middleware
{
    public class TenantResolverMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantResolverMiddleware> _logger;

        public TenantResolverMiddleware(RequestDelegate next, ILogger<TenantResolverMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext, ICurrentTenantService currentTenantService)
        {
            var slug = ExtractTenantSlug(context);

            if (string.IsNullOrWhiteSpace(slug))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            var tenant = await dbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Slug == slug);

            if (tenant is null)
            {
                _logger.LogWarning("Tenant with slug {Slug} not found", slug);
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            if (!tenant.IsActive)
            {
                _logger.LogWarning("Tenant {Slug} is inactive", slug);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            currentTenantService.SetTenant(tenant);
            await _next(context);
        }

        private static string? ExtractTenantSlug(HttpContext context)
        {
            var host = context.Request.Host.Host;
            var segments = host.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > 1)
            {
                return segments[0].ToLowerInvariant();
            }

            var pathSegments = context.Request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            if (pathSegments.Length >= 2 && string.Equals(pathSegments[0], "clinic", StringComparison.OrdinalIgnoreCase))
            {
                return pathSegments[1].ToLowerInvariant();
            }

            return null;
        }
    }
}
