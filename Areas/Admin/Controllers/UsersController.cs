using CabinetMedicalWeb.Areas.Admin.Models;
using CabinetMedicalWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CabinetMedicalWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Sécurité : Seul l'admin peut y accéder
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var modelList = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                modelList.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Nom = user.Nom,
                    Prenom = user.Prenom,
                    Role = roles.FirstOrDefault() ?? "Aucun",
                    Specialite = user.Specialite
                });
            }

            return View(modelList);
        }

        // GET: Admin/Users/Create
        public IActionResult Create()
        {
            var model = new UserViewModel
            {
                // On remplit la liste déroulante avec les rôles (sauf Admin pour éviter les erreurs)
                RolesList = _roleManager.Roles
                    .Where(r => r.Name != "Admin") 
                    .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                    .ToList()
            };
            return View(model);
        }

        // POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            // On retire la validation du mot de passe car on en met un par défaut s'il est vide
            if (string.IsNullOrEmpty(model.Password)) 
            {
                ModelState.Remove("Password");
            }
            
            // On ignore la spécialité si ce n'est pas un médecin
            if (model.Role != "Medecin")
            {
                ModelState.Remove("Specialite");
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Nom = model.Nom,
                    Prenom = model.Prenom,
                    EmailConfirmed = true,
                    // La spécialité est sauvegardée uniquement si le rôle est Médecin
                    Specialite = (model.Role == "Medecin") ? model.Specialite : null
                };

                // Mot de passe par défaut : "Password123!" si le champ est vide
                string passwordToUse = string.IsNullOrEmpty(model.Password) ? "Password123!" : model.Password;

                var result = await _userManager.CreateAsync(user, passwordToUse);

                if (result.Succeeded)
                {
                    // Assignation du rôle
                    await _userManager.AddToRoleAsync(user, model.Role);
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // En cas d'erreur, on recharge la liste des rôles pour réafficher le formulaire
            model.RolesList = _roleManager.Roles
                .Where(r => r.Name != "Admin")
                .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                .ToList();
            
            return View(model);
        }

        // POST: Admin/Users/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}