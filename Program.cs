using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models; // <--- AJOUTER CE NAMESPACE pour ApplicationUser
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace CabinetMedicalWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // Prefer environment override to simplify deployment and local setup
            var useSqlite = builder.Configuration.GetValue("UseSqlite", builder.Environment.IsDevelopment());

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (useSqlite)
                {
                    var sqliteConnection = builder.Configuration.GetConnectionString("SqliteConnection")
                        ?? $"Data Source={Path.Combine(builder.Environment.ContentRootPath, "Data", "cabinetmedical.db")}";

                    var sqliteDataDirectory = Path.GetDirectoryName(sqliteConnection.Replace("Data Source=", string.Empty));
                    if (!string.IsNullOrEmpty(sqliteDataDirectory) && !Directory.Exists(sqliteDataDirectory))
                    {
                        Directory.CreateDirectory(sqliteDataDirectory);
                    }

                    options.UseSqlite(sqliteConnection);
                }
                else
                {
                    var connectionString = builder.Configuration["SQLSERVER_CONNECTION"]
                        ?? builder.Configuration.GetConnectionString("DefaultConnection")
                        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

                    options.UseSqlServer(connectionString);
                }
            });

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // --- CHANGEMENT 1 : UTILISER ApplicationUser ET ACTIVER LES ROLES ---
            // On remplace <IdentityUser> par <ApplicationUser>
            // On ajoute .AddRoles<IdentityRole>() sinon on ne pourra pas cr�er le r�le "Medecin"
            builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
            {
                options.SignIn.RequireConfirmedAccount = false; // Disable email confirmation for easier development
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
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

            app.UseAuthentication();
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
            // C'est ce bloc qui va remplir la base au d�marrage
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