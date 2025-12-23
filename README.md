# GED .NET + Angular - Prototype

Ce dépôt contient un prototype minimal pour une GED (.NET Core API + Angular) qui stocke des pages TIFF en tant que Large Objects (LO) dans PostgreSQL et fournit des previews JPEG pour affichage.

Structure:
- backend/ : API ASP.NET Core
- frontend/ : client Angular minimal
- db/schema.sql : schéma PostgreSQL

Pré-requis:
- .NET 6 SDK (ou .NET 7 modifiable)
- Node.js + npm, Angular CLI (optionnel)
- PostgreSQL 9.6 (ou compatible)
- ImageMagick via NuGet (Magick.NET) est utilisé pour convertir TIFF -> JPEG

Installation:
1. Base de données
   - Exécuter `psql -U <user> -d <db> -f db/schema.sql` pour créer les tables.

2. Backend
   - Ouvrir `backend/` et modifier `appsettings.json` pour la chaîne de connexion.
   - Exécuter:
     ```
     cd backend
     dotnet restore
     dotnet run
     ```
   - L'API sera disponible sur http://localhost:5000 (ou le port indiqué).

3. Frontend (prototype)
   - Ouvrir `frontend/`
     ```
     cd frontend
     npm install
     npx ng serve --open
     ```
   - La page frontend sample affichera un formulaire minimal pour rechercher et uploader des pages.

Notes:
- Les Large Objects (LO) doivent être sauvegardés/restaurés correctement (pg_dump -F c / pg_restore) ; documentez vos procédures de backup.
- Ce prototype n'inclut pas l'authentification/autorisation ni la validation antivirus.
- Pensez à créer des miniatures (thumbnails) lors de l'upload en production pour éviter de convertir le TIFF à chaque visualisation.

Si vous voulez, je peux :
- générer un zip téléchargeable contenant tous ces fichiers,
- ou pousser ce squelette dans un repo GitHub si vous fournissez le repo `owner/name`.
