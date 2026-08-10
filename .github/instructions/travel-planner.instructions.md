---
applyTo: "**/Pages/TravelPlanner/**"
description: "Instructions métier pour le module TravelPlanner"
---

# Module TravelPlanner

## Contexte métier
- Planification de voyages : voyages (`TravelDto`), résumés/lieux (`SummaryDto`), catégories (`CategoryDto`)
- Un voyage contient des résumés classés par statut et catégorie
- Statuts possibles : `ToReview`, `Reviewed`, `Selected`, `Rejected`

## Architecture des composants
- `TravelSidebar` : navigation/filtres (voyages, statuts, catégories, favoris) → communique via `EventCallback` uniquement
- Les filtres sont des `HashSet<T>` (multi-sélection) ou `bool` (favori)
- La sidebar ne détient PAS d'état : tout remonte via `EventCallback` à la page parente

## Conventions spécifiques
- Pills de filtre : `MudPaper Outlined="true"` + classe `PillClass(bool isActive)` → `rounded-xl cursor-pointer` + `mud-border-primary` si actif
- Icônes : version `Filled` si actif, `Outlined` si inactif, même icône dans les deux cas
- Compteurs affichés en `Typo.body2 Color.Secondary` à droite du label
- Les voyages archivés sont regroupés dans une entrée "Archivés" séparée, visible uniquement si `IsArchived` existe
- Ne pas mettre de logique de filtrage dans la sidebar : elle émet, la page filtre
