using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; 

namespace CabinetMedicalWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HorairesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HorairesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================================================================================
        // 1. LISTING DES HORAIRES (INDEX)
        // =========================================================================================
        // Cette action récupère tous les plannings enregistrés en base de données.
        // Elle inclut les informations des employés (Doctor) pour afficher les noms.
        // =========================================================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var horaires = await _context.Horaires
                .Include(h => h.Doctor) // JOINTURE: Indispensable pour accéder à h.Doctor.Nom dans la vue
                .OrderBy(h => h.Doctor.Nom) // Tri alphabétique par nom de famille
                .ToListAsync();

            return View(horaires);
        }

        // =========================================================================================
        // 2. FORMULAIRE DE CRÉATION (GET)
        // =========================================================================================
        // Cette action prépare la vue de création.
        // Son rôle principal est de charger la liste des employés pour le menu déroulant.
        // =========================================================================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Récupération des utilisateurs éligibles (Médecins et Secrétaires)
            // On exclut le compte admin principal pour ne pas lui assigner de shifts médicaux
            var staffMembers = await _userManager.Users
                .Where(u => u.Email != "admin@clinic.com") 
                .Select(u => new 
                { 
                    Id = u.Id, 
                    // Formatage du nom pour l'affichage : "NOM Prénom (Spécialité/Rôle)"
                    FullName = u.Nom + " " + u.Prenom + " (" + (u.Specialite ?? "Secrétariat") + ")"
                })
                .OrderBy(u => u.FullName)
                .ToListAsync();

            // Création de l'objet SelectList pour la vue (Value = Id, Text = FullName)
            ViewData["DoctorId"] = new SelectList(staffMembers, "Id", "FullName");
            
            return View();
        }

        // =========================================================================================
        // 3. ENREGISTREMENT DE L'HORAIRE (POST)
        // =========================================================================================
        // Cette action reçoit les données du formulaire et tente de les sauvegarder.
        // =========================================================================================
        [HttpPost]
        [ValidateAntiForgeryToken] // Protection contre les attaques CSRF
        public async Task<IActionResult> Create([Bind("JoursTravail,HeuresTravail,DoctorId")] Horaire horaire)
        {
            // CRITIQUE : On retire la propriété de navigation "Doctor" de la validation.
            // Le formulaire n'envoie que "DoctorId", donc "Doctor" est null.
            // Sans cette ligne, ModelState.IsValid serait toujours false.
            ModelState.Remove("Doctor");

            if (ModelState.IsValid)
            {
                // Vérification optionnelle : Empêcher les doublons exacts si nécessaire
                // bool exists = await _context.Horaires.AnyAsync(h => h.DoctorId == horaire.DoctorId && h.JoursTravail == horaire.JoursTravail);
                
                _context.Add(horaire);
                await _context.SaveChangesAsync();
                
                // Redirection vers la liste en cas de succès
                return RedirectToAction(nameof(Index));
            }

            // --- CAS D'ERREUR ---
            // Si on arrive ici, c'est que le formulaire est invalide.
            // Il faut RECHARGER la liste déroulante car le protocole HTTP est sans état (stateless).
            var staffMembers = await _userManager.Users
                .Where(u => u.Email != "admin@clinic.com")
                .Select(u => new 
                { 
                    Id = u.Id, 
                    FullName = u.Nom + " " + u.Prenom + " (" + (u.Specialite ?? "Secrétariat") + ")"
                })
                .ToListAsync();
                
            // On remet la valeur sélectionnée par l'utilisateur (horaire.DoctorId) pour ne pas perdre son choix
            ViewData["DoctorId"] = new SelectList(staffMembers, "Id", "FullName", horaire.DoctorId);
            
            return View(horaire);
        }

        // =========================================================================================
        // 4. SUPPRESSION D'UN HORAIRE (POST)
        // =========================================================================================
        // Action appelée lors du clic sur le bouton "Supprimer" dans la liste.
        // =========================================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var horaire = await _context.Horaires.FindAsync(id);
            if (horaire != null)
            {
                _context.Horaires.Remove(horaire);
                await _context.SaveChangesAsync();
            }
            // On reste sur la liste après suppression
            return RedirectToAction(nameof(Index));
        }
    }
}