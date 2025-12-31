using System;
using System.Threading;
using System.Threading.Tasks;
using CabinetMedicalWeb.Models;
using CabinetMedicalWeb.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CabinetMedicalWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ICurrentTenantService _currentTenantService;

        private Guid CurrentTenantId => _currentTenantService.TenantId ?? Guid.Empty;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentTenantService currentTenantService)
            : base(options)
        {
            _currentTenantService = currentTenantService;
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<RendezVous> RendezVous { get; set; }
        public DbSet<DossierMedical> Dossiers { get; set; }
        public DbSet<Consultation> Consultation { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<ResultatExamen> ResultatExamens { get; set; }
        public DbSet<Horaire> Horaires { get; set; }
        public DbSet<Conge> Conges { get; set; }
        public DbSet<ReservationRequest> ReservationRequests { get; set; }
        public DbSet<Tenant> Tenants { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Conge>()
                .Property(c => c.Status)
                .HasDefaultValue(CongeStatus.Pending);

            builder.Entity<Tenant>()
                .HasIndex(t => t.Slug)
                .IsUnique();

            builder.Entity<Patient>().HasQueryFilter(p => p.TenantId == CurrentTenantId);
            builder.Entity<RendezVous>().HasQueryFilter(r => r.TenantId == CurrentTenantId);
            builder.Entity<DossierMedical>().HasQueryFilter(d => d.TenantId == CurrentTenantId);
            builder.Entity<Consultation>().HasQueryFilter(c => c.TenantId == CurrentTenantId);
            builder.Entity<Prescription>().HasQueryFilter(p => p.TenantId == CurrentTenantId);
            builder.Entity<ResultatExamen>().HasQueryFilter(r => r.TenantId == CurrentTenantId);
            builder.Entity<Horaire>().HasQueryFilter(h => h.TenantId == CurrentTenantId);
            builder.Entity<ReservationRequest>().HasQueryFilter(r => r.TenantId == CurrentTenantId);
            builder.Entity<Conge>().HasQueryFilter(c => c.TenantId == CurrentTenantId);
            builder.Entity<ApplicationUser>().HasQueryFilter(u => u.TenantId == CurrentTenantId);

            builder.Entity<Patient>().HasIndex(p => p.TenantId);
            builder.Entity<RendezVous>().HasIndex(r => new { r.TenantId, r.DateHeure });
            builder.Entity<Consultation>().HasIndex(c => new { c.TenantId, c.Date });
            builder.Entity<Prescription>().HasIndex(p => new { p.TenantId, p.DatePrescription });
            builder.Entity<ResultatExamen>().HasIndex(r => new { r.TenantId, r.DateExamen });
            builder.Entity<ReservationRequest>().HasIndex(r => new { r.TenantId, r.DateSouhaitee });
            builder.Entity<Horaire>().HasIndex(h => h.TenantId);
            builder.Entity<Conge>().HasIndex(c => c.TenantId);
            builder.Entity<ApplicationUser>().HasIndex(u => u.TenantId);
        }

        public override int SaveChanges()
        {
            EnforceTenantIds();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            EnforceTenantIds();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void EnforceTenantIds()
        {
            var tenantId = _currentTenantService.TenantId;
            foreach (var entry in ChangeTracker.Entries<TenantEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    if (!tenantId.HasValue)
                    {
                        throw new InvalidOperationException("Tenant context is required to create tenant-scoped entities.");
                    }

                    entry.Entity.TenantId = tenantId.Value;
                }

                if (entry.State == EntityState.Modified && entry.Property(e => e.TenantId).IsModified)
                {
                    entry.Property(e => e.TenantId).IsModified = false;
                }
            }

            foreach (var entry in ChangeTracker.Entries<ApplicationUser>())
            {
                if (entry.State == EntityState.Added)
                {
                    if (!tenantId.HasValue)
                    {
                        throw new InvalidOperationException("Tenant context is required to create users.");
                    }

                    entry.Entity.TenantId = tenantId.Value;
                }

                if (entry.State == EntityState.Modified && entry.Property(u => u.TenantId).IsModified)
                {
                    entry.Property(u => u.TenantId).IsModified = false;
                }
            }
        }
    }
}