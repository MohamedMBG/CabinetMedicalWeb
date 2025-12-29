
# CabinetMedicalWeb – Documentation Complète

## 1. Présentation générale
CabinetMedicalWeb est une application ASP.NET Core MVC 8.0 dédiée à la gestion d'un cabinet médical. Elle s'appuie sur une architecture à zones (Areas) pour séparer les rôles métier (accueil, médical et administration), s'interface avec SQL Server via Entity Framework Core et utilise ASP.NET Identity pour l'authentification et la gestion des rôles. Le projet propose également des services d'envoi de mails SMTP et de stockage d'images Cloudinary pour les examens.

## 2. Démarrage rapide
1. **Prérequis** : .NET 8 SDK, SQL Server, et des clés Cloudinary/SMTP adaptées à votre environnement.
2. **Configuration** : ajustez `DefaultConnection`, la section `Smtp` et les secrets `Cloudinary` dans `appsettings.json` ou via des variables d'environnement avant le déploiement.
3. **Restauration et migration** :
   ```bash
   dotnet restore
   dotnet ef database update
   ```
   Les migrations EF Core présentes dans `Data/Migrations` créent les tables Patients, RendezVous, Dossiers, Consultations, Prescriptions, Examens, Horaires, Congés et Réservations.
4. **Lancement** : démarrez l'application avec `dotnet run` à la racine du projet. Les routes sont préconfigurées pour gérer les Areas et les pages Razor d'identité.
5. **Données de base (optionnel)** : le seeding Identity (rôles Admin/Medecin/Secretaire et comptes par défaut) est prêt dans `DbInitializer.Initialize` et peut être réactivé depuis `Program.cs` si nécessaire.

## 3. Architecture et configuration
- **Pipeline et services** : `Program.cs` configure l'ApplicationDbContext avec la connexion SQL Server, active ASP.NET Identity (password policy assouplie et rôles), enregistre le service SMTP (`IEmailService`), le service Cloudinary (`ICloudinaryService`), les contrôleurs MVC et les routes d'Areas.
- **Zones (Areas)** :
  - `FrontDesk` regroupe les contrôleurs et vues de l'accueil (agenda, patients, planning, rendez-vous, demandes de réservation).
  - `Medical` contient les fonctionnalités médicales (dossiers, consultations, prescriptions, examens, laboratoire, tableau de bord médecins).
  - `Admin` propose le pilotage (tableau de bord statistiques, gestion des utilisateurs, horaires et congés).
  - `Identity` expose les pages Razor générées pour la gestion de compte et la connexion.
- **Services applicatifs** :
  - `SmtpEmailService` envoie des e-mails HTML via les paramètres `Smtp` (désactivable via `Enabled`).
  - `CloudinaryService` charge des scans d'examens et renvoie l’URL sécurisée et le `PublicId`.

## 4. Modèle de données principal
- **ApplicationUser** : étend `IdentityUser` avec `Nom`, `Prenom` et une `Specialite` optionnelle pour distinguer médecins et secrétaires.
- **Patient** : identité civile, coordonnées et antécédents, avec un lien 1–1 vers `DossierMedical`.
- **DossierMedical** : racine clinique d’un patient, agrège `Consultations`, `Prescriptions` et `ResultatExamens`.
- **Consultation** : date, motif, notes et auteur (`DoctorId`) reliés à un dossier.
- **Prescription** : date, liste de médicaments, dossier associé et médecin prescripteur.
- **ResultatExamen** : type/date de l’examen, résultat, métadonnées Cloudinary (`ScanUrl`, `ScanPublicId`) et dossier associé.
- **RendezVous** : rendez-vous planifié avec motif/statut, lié à un patient et à un médecin (`DoctorId`).
- **Horaire** : jours et créneaux de travail d’un médecin.
- **Conge** : plage de dates, motif optionnel, statut (Pending/Approved/Rejected) et personnel concerné.
- **ReservationRequest** : demande publique de consultation (informations de contact, date souhaitée, motif, statut, patient/médecin/slot confirmés facultatifs). `ReservationStatus` centralise les valeurs possibles.

## 5. Flux applicatifs clés
- **Demandes publiques de rendez-vous** : `ReservationsController` expose un tableau de bord patient (prérempli avec l’email) et enregistre les demandes valides dans `ReservationRequests` avec le statut `PENDING` avant traitement par l’accueil.
- **Accueil (FrontDesk)** :
  - `RendezVousController` prépare la création de rendez-vous (préremplissage de la date, statut « Planifié », listes médecins/patients filtrées sur le rôle "Medecin") et refuse la sauvegarde en cas de conflit d’horaire sur le médecin.
  - Les autres contrôleurs de la zone couvrent la gestion de l’agenda, des patients, du planning et des demandes de réservation.
- **Espace médical (Medical)** :
  - `DossierMedicalsController` liste, recherche et affiche les dossiers avec leurs consultations/prescriptions/examens, crée un dossier unique par patient et construit une vue détaillée ordonnée par date.
  - `ConsultationController`, `PrescriptionController` et `LaboController` complètent la saisie clinique et la gestion des examens (dont l’upload Cloudinary).
- **Administration** :
  - `DashboardController` calcule des statistiques (patients, dossiers, prescriptions quotidiennes, performances hebdo/mensuelles et par médecin/réception) pour alimenter la vue de pilotage.
  - `UsersController` permet de lister, créer, modifier et supprimer les comptes du personnel en attribuant rôles et spécialités.
  - `HorairesController` et `CongesController` assurent la gestion des disponibilités et absences des médecins.

## 6. Sécurité et authentification
- L’application utilise ASP.NET Identity avec rôles `Admin`, `Medecin` et `Secretaire` pour restreindre l’accès aux zones sensibles (contrôleurs `Medical` et `Admin` décorés d’`[Authorize]`).
- Les options de mot de passe exigent des majuscules/minuscules et un chiffre, sans caractère spécial obligatoire (longueur minimale 6).
- Les pages d’identité Razor sont exposées via `MapRazorPages` ; l’inscription/connexion passe par l’interface standard générée.

## 7. Paramétrage des services externes
- **SMTP** : configurez l’hôte, le port, SSL, l’expéditeur et les identifiants dans `appsettings.json` (ou variables d’environnement) ; le flag `Enabled` permet de désactiver l’envoi en développement.
- **Cloudinary** : fournissez `CloudName`, `ApiKey` et `ApiSecret` ; les fichiers d’examens sont téléversés dans le dossier spécifié lors de l’appel service et retournent l’URL sécurisée et le `PublicId` pour un éventuel remplacement/suppression.

## 8. Tests et bonnes pratiques
- **Conflits d’agenda** : la création de rendez-vous vérifie l’unicité d’un créneau par médecin.
- **Validation** : les modèles incluent des annotations de validation pour les données patients, réservations et congés ; les actions POST renvoient les vues avec erreurs si le modèle est invalide.
- **Évolutivité** : ajoutez de nouvelles entités dans `ApplicationDbContext` puis générez une migration EF Core ; en production, activez le seeding contrôlé via `DbInitializer` pour provisionner rôles/comptes.