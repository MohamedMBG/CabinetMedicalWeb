using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models;
using CabinetMedicalWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CabinetMedicalWeb.Areas.Medical.Controllers
{
    [Area("Medical")]
    public class LaboController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<LaboController> _logger;

        public LaboController(ApplicationDbContext context, IEmailService emailService, ILogger<LaboController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: Medical/Labo/Create?dossierId=5
        public IActionResult Create(int dossierId)
        {
            if (dossierId == 0)
            {
                return RedirectToAction("Index", "DossierMedicals");
            }

            var resultat = new ResultatExamen
            {
                DossierMedicalId = dossierId,
                DateExamen = DateTime.Now
            };
            return View(resultat);
        }

        // POST: Medical/Labo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ResultatExamen resultatExamen, bool sendEmail)
        {
            // CRITIQUE : Retirer la validation de la propriété de navigation
            // Le formulaire n'envoie que l'ID, donc "DossierMedical" est null
            ModelState.Remove("DossierMedical");

            // Vérification de l'existence du dossier
            var dossier = await _context.Dossiers
                .Include(d => d.Patient)
                .FirstOrDefaultAsync(d => d.Id == resultatExamen.DossierMedicalId);

            if (dossier is null)
            {
                ModelState.AddModelError("", "Dossier médical introuvable.");
            }

            if (ModelState.IsValid && dossier is not null)
            {
                try 
                {
                    _context.Add(resultatExamen);
                    await _context.SaveChangesAsync();

                    if (sendEmail && dossier.Patient is not null && !string.IsNullOrWhiteSpace(dossier.Patient.Email))
                    {
                        var subject = $"Nouveau résultat d'examen ({resultatExamen.TypeExamen})";
                        var body =
                            $"Bonjour {dossier.Patient.Prenom} {dossier.Patient.Nom},\n\n" +
                            $"Votre résultat d'examen est disponible.\n" +
                            $"Date de l'examen : {resultatExamen.DateExamen:dd/MM/yyyy HH:mm}.\n" +
                            $"Type d'examen : {resultatExamen.TypeExamen}.\n\n" +
                            "Résultats :\n" + resultatExamen.Resultat + "\n\n" +
                            "Merci de vous connecter à votre espace patient ou de contacter le cabinet pour plus de détails.";

                        try
                        {
                            await _emailService.SendEmailAsync(dossier.Patient.Email, subject, body);
                        }
                        catch (Exception ex)
                        {
                            // On log l'erreur mais on ne bloque pas l'enregistrement du résultat
                            _logger.LogError(ex, "Erreur lors de l'envoi du mail de résultat pour le dossier {DossierId}", dossier.Id);
                        }
                    }

                    // Retour au Dashboard du patient
                    return RedirectToAction("Details", "DossierMedicals", new { id = resultatExamen.DossierMedicalId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la sauvegarde du résultat labo");
                    ModelState.AddModelError("", "Une erreur est survenue lors de l'enregistrement.");
                }
            }

            return View(resultatExamen);
        }
    }
}