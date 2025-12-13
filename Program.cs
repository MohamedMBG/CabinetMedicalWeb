using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models; // <--- AJOUTER CE NAMESPACE pour ApplicationUser
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CabinetMedicalWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // --- CHANGEMENT 1 : UTILISER ApplicationUser ET ACTIVER LES ROLES ---
            // On remplace <IdentityUser> par <ApplicationUser>
            // On ajoute .AddRoles<IdentityRole>() sinon on ne pourra pas créer le rôle "Medecin"
            builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();

            // --- CHANGEMENT 2 : ACTIVER LES "AREAS" (SEPARATION FRONTDESK / MEDICAL) ---
            // Ceci est obligatoire pour que ASP.NET trouve vos dossiers "Areas"
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.MapRazorPages()
               .WithStaticAssets();

            // --- CHANGEMENT 3 : LE SEEDER AUTOMATIQUE ---
            // C'est ce bloc qui va remplir la base au démarrage
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    // On lance l'initialisation (ATTENTION : DbInitializer doit exister dans Data/)
                    DbInitializer.Initialize(context, userManager, roleManager).Wait();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Erreur lors du remplissage automatique de la base (Seeding).");
                }
            }

            app.Run();
        }
    }
}