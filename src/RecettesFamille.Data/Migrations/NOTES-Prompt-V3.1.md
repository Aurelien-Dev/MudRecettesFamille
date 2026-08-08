# Amélioration du prompt V3.1 - Structure Markdown renforcée

## Problème identifié

Le prompt original demandait 3 éléments dans le résumé :
1. Résumé court
2. Liste d'astuces/conseils
3. Liste des lieux

**Mais l'IA ne générait souvent que le résumé court**, en ignorant les sections 2 et 3.

## Solution appliquée

Le prompt a été **restructuré de manière directive** avec :

### ✅ Structure Markdown OBLIGATOIRE

```markdown
Résumé général (paragraphe)

## Conseils et astuces pratiques
- Liste des conseils

## Lieux mentionnés
- Liste des lieux
```

### ✅ Instructions très claires

- 3 sections **OBLIGATOIRES** numérotées
- Titres Markdown **explicites** (`##`)
- **Exemple concret** de la structure attendue
- **Cas de fallback** : "Aucun conseil/lieu mentionné" si vide
- Répétition des règles : ne pas inventer

### ✅ Catégorisation des conseils

Les conseils sont maintenant organisés en sous-catégories :
- **Transport**
- **Logement**
- **Budget**
- **Erreurs à éviter**
- **Astuces pratiques**

### ✅ Format standardisé pour les lieux

Format attendu : `**Nom du lieu** (Adresse) - Type/description`

Exemple :
```markdown
- **Yakiniku Jumbo Hanare** (3-14-9 Roppongi, Minato-ku) - Restaurant de viande grillée primé
```

## Fichiers modifiés

1. **Code** : `src/RecettesFamille/Managers/AiGenerators/AiManager.cs`
   - Constante `YOUTUBE_RESUME_V3_PROMPT` mise à jour

2. **Documentation** :
   - `YoutubeResumeV3_Prompt.txt` : Texte complet du prompt
   - `insert-prompt-v3.sql` : Script SQL pour mise à jour en BDD

## Déploiement

### Option 1 : Utiliser le prompt hardcodé (par défaut)
✅ **Aucune action nécessaire**

Le prompt est déjà dans le code et sera utilisé automatiquement.

### Option 2 : Stocker en base de données (optionnel)

```bash
psql -h <host> -U <username> -d <database> -f src/RecettesFamille.Data/Migrations/insert-prompt-v3.sql
```

Le code essaiera d'abord la BDD, puis utilisera le fallback hardcodé si absent.

## Test

Créer un nouveau résumé et vérifier que le résumé contient **systématiquement** :

1. ✅ Un paragraphe d'introduction
2. ✅ Section `## Conseils et astuces pratiques` (même si vide)
3. ✅ Section `## Lieux mentionnés` (même si vide)

## Exemple de résultat attendu

```markdown
La vidéo présente un voyage à Tokyo pendant la saison des cerisiers en fleurs, 
avec un focus sur la gastronomie locale et les quartiers traditionnels.

## Conseils et astuces pratiques
- **Transport** : Acheter une Suica Card dès l'arrivée
- **Budget** : Prévoir 15-25€ par repas
- **Astuces** : Éviter les restaurants près des gares

## Lieux mentionnés
- **Yakiniku Jumbo Hanare** (3-14-9 Roppongi) - Restaurant de viande
- **Jardin Shinjuku Gyoen** - Parc pour les cerisiers
```

## Différence avec l'ancien prompt

| Aspect | Ancien prompt | Nouveau prompt |
|--------|--------------|----------------|
| Structure | Instructions numérotées vagues | 3 sections Markdown OBLIGATOIRES |
| Exemple | Aucun | Exemple concret fourni |
| Titres | Pas spécifiés | Titres Markdown imposés (`##`) |
| Cas vide | Non géré | "Aucun conseil mentionné" |
| Organisation | Informations mélangées | Conseils catégorisés (Transport, Budget, etc.) |

## Impact attendu

✅ **100% des résumés** auront maintenant les 3 sections structurées
✅ Meilleure lisibilité avec les titres Markdown
✅ Facilite l'extraction future de données structurées (V3.2+)
