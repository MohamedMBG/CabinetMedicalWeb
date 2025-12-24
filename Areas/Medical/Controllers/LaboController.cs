using CabinetMedicalWeb.Data;
using CabinetMedicalWeb.Models;
using CabinetMedicalWeb.Services;
using Microsoft.AspNetCore.Http;
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
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<LaboController> _logger;

        public LaboController(
            ApplicationDbContext context, 
            IEmailService emailService, 
            ICloudinaryService cloudinaryService,
            ILogger<LaboController> logger)
        {
            _context = context;
            _emailService = emailService;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        // -----------------------------------------------------------
        // NEW: GET: Medical/Labo/Index?dossierId=5
        // Shows the full gallery of lab results for a patient
        // -----------------------------------------------------------
        public async Task<IActionResult> Index(int dossierId)
        {
            if (dossierId == 0) return RedirectToAction("Index", "DossierMedicals");

            var dossier = await _context.Dossiers
                .Include(d => d.Patient)
                .FirstOrDefaultAsync(d => d.Id == dossierId);

            if (dossier == null) return NotFound();

            ViewBag.PatientName = $"{dossier.Patient.Nom} {dossier.Patient.Prenom}";
            ViewBag.DossierId = dossierId;

            var exams = await _context.ResultatExamens
                .Where(e => e.DossierMedicalId == dossierId)
                .OrderByDescending(e => e.DateExamen)
                .ToListAsync();

            return View(exams);
        }

        // GET: Medical/Labo/Create
        public IActionResult Create(int dossierId)
        {
            if (dossierId == 0) return RedirectToAction("Index", "DossierMedicals");

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
        public async Task<IActionResult> Create(ResultatExamen resultatExamen, bool sendEmail, IFormFile? scanFile)
        {
            // DEBUG: Vérifier les paramètres reçus
            Console.WriteLine($"[DEBUG EMAIL] Demande création reçue. SendEmail = {sendEmail}");

            ModelState.Remove("DossierMedical");

            var dossier = await _context.Dossiers
                .Include(d => d.Patient)
                .FirstOrDefaultAsync(d => d.Id == resultatExamen.DossierMedicalId);

            if (dossier == null) ModelState.AddModelError("", "Dossier introuvable.");

            if (ModelState.IsValid && dossier != null)
            {
                try 
                {
                    // 1. SAUVEGARDE DB INITIALE
                    _context.Add(resultatExamen);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"[DEBUG DB] Résultat sauvegardé ID: {resultatExamen.IdResultat}");

                    // 2. UPLOAD CLOUDINARY
                    if (scanFile != null && scanFile.Length > 0)
                    {
                        try {
                            string folderPath = $"patients/{dossier.PatientId}/lab-results/{resultatExamen.IdResultat}";
                            var uploadResult = await _cloudinaryService.UploadScanAsync(scanFile, folderPath);
                            
                            resultatExamen.ScanUrl = uploadResult.Url;
                            resultatExamen.ScanPublicId = uploadResult.PublicId;
                            
                            _context.Update(resultatExamen);
                            await _context.SaveChangesAsync();
                            Console.WriteLine($"[DEBUG CLOUD] Image uploadée: {uploadResult.Url}");
                        }
                        catch(Exception ex) {
                            Console.WriteLine($"[DEBUG CLOUD ERROR] {ex.Message}");
                        }
                    }

                    // 3. ENVOI EMAIL (C'est ici que ça se joue)
                    if (sendEmail)
                    {
                        if (string.IsNullOrWhiteSpace(dossier.Patient.Email))
                        {
                            Console.WriteLine("[DEBUG EMAIL] Annulé : Le patient n'a pas d'email.");
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG EMAIL] Tentative d'envoi à {dossier.Patient.Email}...");
                            
                            // Préparation du contenu visuel (Image ou PDF)
                            string visualContent = "";
                            string downloadButton = "";

                            if (!string.IsNullOrEmpty(resultatExamen.ScanUrl))
                            {
                                downloadButton = $"<br><br><a href='{resultatExamen.ScanUrl}' style='display:inline-block;padding:12px 24px;background-color:#10B981;color:white;text-decoration:none;border-radius:6px;font-weight:bold;'>📥 Voir le document</a>";

                                if (resultatExamen.ScanUrl.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                                {
                                    visualContent = "<div style='background-color:#f3f4f6;padding:15px;text-align:center;'>📄 Document PDF joint</div>";
                                }
                                else
                                {
                                    visualContent = $"<div style='margin-top:20px;text-align:center;'><img src='{resultatExamen.ScanUrl}' style='max-width:100%;border-radius:8px;' /></div>";
                                }
                            }

                            var subject = $"Résultat disponible : {resultatExamen.TypeExamen}";
                            var body = $@"
                                <h2>Nouveau Résultat d'Examen</h2>
                                <p>Bonjour <strong>{dossier.Patient.Nom}</strong>,</p>
                                <p>Voici le résultat du {resultatExamen.DateExamen:dd/MM/yyyy} :</p>
                                <div style='background:#f9f9f9;padding:15px;border-left:4px solid #10B981;'>
                                    <strong>{resultatExamen.TypeExamen}</strong><br>
                                    {resultatExamen.Resultat}
                                </div>
                                {visualContent}
                                {downloadButton}";

                            try {
                                await _emailService.SendEmailAsync(dossier.Patient.Email, subject, body);
                                Console.WriteLine("[DEBUG EMAIL] SUCCÈS : Email envoyé !");
                            }
                            catch (Exception ex) {
                                Console.WriteLine($"[DEBUG EMAIL ERROR] Échec : {ex.Message}");
                                // Si c'est une erreur 11004 ou Socket, c'est le réseau/pare-feu
                            }
                        }
                    }

                    return RedirectToAction("Details", "DossierMedicals", new { id = resultatExamen.DossierMedicalId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur globale");
                    ModelState.AddModelError("", "Erreur technique.");
                }
            }
            return View(resultatExamen);
        }
    }
}