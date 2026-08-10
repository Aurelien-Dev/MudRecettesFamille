# Instructions globales – MudRecettesFamille

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
