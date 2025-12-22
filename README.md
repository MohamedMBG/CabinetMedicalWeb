# 🏥 CabinetMedicalWeb

Bienvenue sur le projet **CabinetMedicalWeb**. 
Ceci est une application web de gestion de cabinet médical développée en **ASP.NET Core MVC (version 8.0)**. 

Le projet utilise une architecture **Multi-Zones (Areas)** pour séparer distinctement la gestion administrative (Secrétariat) de la gestion médicale (Médecins).

---

## 🚀 Fonctionnalités

L'application est divisée en deux espaces de travail distincts :

### 1. Espace Accueil (Area: FrontDesk)
*Destiné au personnel administratif (Secrétaires).*
* **Gestion des Patients :** Enregistrement, modification et listing des patients.
* **Agenda :** Visualisation du calendrier des rendez-vous.
* **Prise de RDV :** Création de nouveaux rendez-vous pour les patients.
* **Planning :** Gestion des créneaux horaires des médecins.

### 2. Espace Médical (Area: Medical)
*Destiné au personnel soignant (Médecins).*
* **Dossier Médical :** Vue centrale de l'historique d'un patient.
* **Consultations :** Saisie des comptes-rendus de visite.
* **Prescriptions :** Création d'ordonnances liées aux consultations.
* **Laboratoire :** Gestion et upload des résultats d'examens.

---

## 🛠️ Stack Technique

* **Framework :** ASP.NET Core 8.0
* **Architecture :** MVC (Model-View-Controller) avec Areas
* **Base de données :** SQL Server
* **ORM :** Entity Framework Core
* **Authentification :** ASP.NET Core Identity (Gestion des utilisateurs et rôles)
* **Frontend :** Razor Views, Bootstrap 5

---

## 🏗️ Structure du Projet

L'architecture est conçue pour faciliter le travail en binôme sans conflits :

```text
CabinetMedicalWeb
│
├── 📁 Models               # (⚠️ ZONE PARTAGÉE) Entités de la BDD (Patient, RDV, Dossier...)
├── 📁 Data                 # Configuration BDD (DbContext) et Seeder
│
├── 📁 Areas
│   ├── 📁 FrontDesk        # 👤 Zone du Développeur A (Logique Administrative)
│   │   ├── Controllers
│   │   ├── Models (ViewModels)
│   │   └── Views
│   │
│   └── 📁 Medical          # 🩺 Zone du Développeur B (Logique Médicale)
│       ├── Controllers
│       ├── Models (ViewModels)
│       └── Views
│
└── 📁 wwwroot              # Fichiers statiques (CSS, JS, Images)
