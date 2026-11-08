# Instructions globales – MudRecettesFamille

## Architecture - Recettes Famille

Cette solution contient le site recettes et ses dependances autonomes.

### Recettes Famille
- Pages Blazor : `src/RecettesFamille/Components/Pages/Recettes/`
- Entités EF : `src/RecettesFamille.Data/EntityModel/` (RecipeEntity, TagEntity, BlockBaseEntity…)
- DTOs : `src/RecettesFamille.Dto/Models/` et `ModelByPage/`
- Repository : `src/RecettesFamille.Data.Repository/Repositories/`

YouSummarize (TravelPlanner) est une solution distincte dans `D:\01_dev\YouSummarize`.
Ne pas ajouter de references de projets ou de fichiers entre les deux solutions.
La base PostgreSQL reste partagee. Le modele EF et l'historique des migrations de
ce depot sont conserves : ne pas retirer les mappings voyages ni generer de
migration destructive pour nettoyer le code. Aucun changement de donnees n'est
necessaire pour le split.

## Stack technique
- Blazor .NET 10, composants Server-side
- MudBlazor pour TOUS les composants UI : ne jamais utiliser Bootstrap, HTML natif (`<div>`, `<button>`, `<input>`, etc.) ou CSS custom quand un équivalent MudBlazor existe
- Langue de l'interface : français (labels, messages, titres)
- Langue du code : anglais (noms de variables, méthodes, classes, commentaires)

## Utilisation de MudBlazor — règles strictes
- Layout : toujours `MudStack`, `MudGrid`, `MudItem` — jamais de `<div class="row">` ou flexbox custom
- Boutons : toujours `MudButton` ou `MudIconButton` — jamais `<button>`
- Texte : toujours `MudText` avec le bon `Typo.*` — jamais `<p>`, `<h1>`…`<h6>`, `<span>` isolé
- Icônes : toujours `MudIcon` ou via `Icon=` sur les composants — toujours `Icons.Material.Filled.*` (actif) / `Icons.Material.Outlined.*` (inactif)
- Formulaires : toujours `MudForm` + `MudTextField`, `MudSelect`, `MudCheckBox`, `MudSwitch`, etc.
- Dialogues : toujours `MudDialog` via `IDialogService` — jamais de modale custom
- Notifications : toujours `ISnackbar` — jamais d'alert HTML
- Tableaux : toujours `MudTable` ou `MudDataGrid`
- Navigation : toujours `MudNavMenu` + `MudNavLink`
- Séparateurs : toujours `MudDivider`
- Cartes : toujours `MudPaper` ou `MudCard` selon le besoin
- Chargement : toujours `MudProgressCircular` ou `MudSkeleton`
- Tooltips : toujours `MudTooltip`

## Conventions générales
- Les composants Blazor sont découpés en sous-composants dans un dossier `<PageName>PageComponents/`
- Les `@code` blocks restent dans le `.razor` sauf logique complexe → fichier `.razor.cs` code-behind
- Privilégier les `EventCallback` pour la communication enfant → parent
- Toujours utiliser les DTOs (suffixe `Dto`) pour les échanges entre couches
- Pas de logique métier dans les composants UI : déléguer aux services injectés
- Avant de proposer des modifications importantes (changement d'image de base, refactoring architectural, etc.), toujours discuter des options et demander confirmation à l'utilisateur avant de toucher au code.
