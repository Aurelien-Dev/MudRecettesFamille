# RecettesFamille

RecettesFamille est une application web pour gérer et partager des recettes de cuisine. Elle utilise ASP.NET Core, Blazor et MudBlazor pour l'interface utilisateur, et PostgreSQL pour la base de données.

## Fonctionnalités

- Inscription et connexion des utilisateurs
- Gestion des recettes (ajout, modification, suppression)
- Importation de recettes via GPT
- Confirmation par email
- Gestion des ingrédients et des instructions de recette
- Génération d'images de recettes avec OpenAI
- Recherche sémantique filtrable par nom, tag ou ingrédient
- Les résultats de recherche incluent maintenant l'identifiant de la recette

## Structure du projet

- `src/RecettesFamille` : Contient le code principal de l'application
- `src/RecettesFamille.Data` : Contient les modèles de données et les migrations Entity Framework
- `src/RecettesFamille.Dto` : Contient les objets de transfert de données (DTO)
- `src/RecettesFamille.Data.Repository` : Contient les interfaces et implémentations des dépôts de données
- `src/RecettesFamille.Docker` : Contient les fichiers de configuration Docker

## Prérequis

- .NET 9.0 SDK
- PostgreSQL
- Docker (optionnel)

## Installation

1. Clonez le dépôt :
    ```sh
    git clone https://github.com/votre-utilisateur/RecettesFamille.git
    cd RecettesFamille
    ```

2. Configurez la base de données PostgreSQL et mettez à jour la chaîne de connexion dans `appsettings.json`.

3. Appliquez les migrations de la base de données :
    ```sh
    dotnet ef database update --project src/RecettesFamille.Data
    ```

4. Lancez l'application :
    ```sh
    dotnet run --project src/RecettesFamille
    ```

## Utilisation

- Accédez à l'application via `http://localhost:5158` ou `https://localhost:7225`.
- Inscrivez-vous et connectez-vous pour commencer à ajouter et gérer des recettes.

## Déploiement avec Docker

1. Construisez et lancez les conteneurs Docker :
    ```sh
    docker-compose -f src/RecettesFamille.Docker/docker-compose.yml up --build -d
    ```

2. Accédez à l'application via l'adresse configurée dans Docker.


## Tips

# Modify the server-side reconnection handler

https://learn.microsoft.com/fr-fr/aspnet/core/blazor/fundamentals/signalr?view=aspnetcore-8.0#modify-the-server-side-reconnection-handler

# ThemeManager :

Editeur de thème : https://themes.arctechonline.tech/