using CabinetMedicalWeb.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CabinetMedicalWeb.Data
{
    // NOTICE: We use <ApplicationUser> here!
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<RendezVous> RendezVous { get; set; }
        public DbSet<DossierMedical> Dossiers { get; set; }
        // Add your other tables here
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<ResultatExamen> ResultatExamens { get; set; }
        public DbSet<Horaire> Horaires { get; set; }
    }
}