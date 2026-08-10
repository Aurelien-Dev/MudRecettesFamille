---
applyTo: "**/Pages/Recettes/**,**/Pages/RecetteDetail/**"
description: "Instructions métier pour le module RecettesFamille"
---

# Module RecettesFamille

## Contexte métier
- Gestion de recettes familiales : ingrédients, étapes, catégories, photos
- Entités principales : `RecetteDto`, `IngredientDto`, `EtapeDto`

## Conventions spécifiques
- Les listes de recettes utilisent des filtres par catégorie et recherche textuelle
- Les formulaires de création/édition de recette utilisent un stepper MudBlazor (`MudStepper`)
- Les images sont stockées et référencées via un service dédié, ne pas gérer les fichiers directement dans les composants
- Valider les formulaires avec `MudForm` et sa méthode `Validate()` avant sauvegarde
