using CabinetMedicalWeb.Areas.Admin.Controllers;
using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models;
using CabinetMedicalWeb.Services; // <--- Namespace de nos nouveaux fichiers
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

            builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
            {
                options.SignIn.RequireConfirmedAccount = false; 
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            
            // --- CONFIGURATION EMAIL CORRIGÉE ---
            // On lie la section "Smtp" du fichier appsettings.json à la classe SmtpSettings
            builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp")); 
            
            // On injecte le service. Maintenant ça marche car SmtpEmailService hérite bien de IEmailService
            builder.Services.AddTransient<IEmailService, SmtpEmailService>();

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

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    
                    // Assurez-vous que DbInitializer est bien accessible ici
                    // DbInitializer.Initialize(context, userManager, roleManager).Wait(); 
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