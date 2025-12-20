using CabinetMedicalWeb.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CabinetMedicalWeb.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            context.Database.EnsureCreated();

            // Si des utilisateurs existent déjà, on ne fait rien
            if (context.Users.Any()) return;

            // 1. Création des Rôles
            string[] roles = new string[] { "Admin", "Medecin", "Secretaire" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Création de l'administrateur
            var admin = new ApplicationUser
            {
                UserName = "admin@clinic.com",
                Email = "admin@clinic.com",
                Nom = "Admin",
                Prenom = "Principal",
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(admin.Email) == null)
            {
                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // 3. Création du Médecin (Dr. House)
            var doctor = new ApplicationUser
            {
                UserName = "doc@clinic.com",
                Email = "doc@clinic.com",
                Nom = "House",
                Prenom = "Gregory",
                //UserType = "Medecin", // Assurez-vous d'avoir ce champ dans ApplicationUser ou utilisez Specialite
                Specialite = "Diagnostiqueur",
                EmailConfirmed = true
            };

            // ATTENTION : Si la propriété 'UserType' n'existe pas dans votre ApplicationUser, supprimez la ligne 'UserType = ...' ci-dessus.

            await userManager.CreateAsync(doctor, "Password123!");
            await userManager.AddToRoleAsync(doctor, "Medecin");

            // 3. Création des Patients (Utilisez context.Patients.Add...)
            // (Assurez-vous que la classe Patient existe dans Models)
        }
    }
}