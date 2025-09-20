using RecettesFamille.Dto.Models;
using RecettesFamille.Dto.Models.Blocks;
using RecettesFamille.Managers.AiGenerators.Models;

namespace RecettesFamille.Managers.AiGenerators;

/// <summary>
/// Interface pour la gestion des fonctionnalités d'intelligence artificielle.
/// Fournit des méthodes pour convertir des recettes, générer des images, calculer des calories et créer des résumés YouTube.
/// </summary>
public interface IAiManager
{
    /// <summary>
    /// Convertit un texte brut en une structure de recette complète à l'aide de l'intelligence artificielle.
    /// </summary>
    /// <param name="recipe">Le texte brut de la recette à convertir.</param>
    /// <param name="aiClientTypeEnum">Le type de client AI à utiliser pour la conversion.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Une structure de recette complète avec tous ses composants.</returns>
    Task<RecipeDto> ConvertRecipe(string recipe, AiClientType aiClientTypeEnum, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Demande à l'intelligence artificielle de générer une image pour une recette spécifique.
    /// </summary>
    /// <param name="recipeId">L'identifiant de la recette pour laquelle générer une image.</param>
    /// <param name="includeFullRecipe">Indique si la recette complète doit être utilisée pour la génération.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>L'URL ou les données de l'image générée.</returns>
    Task<string> AskImage(int recipeId, bool includeFullRecipe, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Calcule le nombre approximatif de calories pour une liste d'ingrédients à l'aide de l'intelligence artificielle.
    /// </summary>
    /// <param name="ingredients">La liste des ingrédients pour lesquels calculer les calories.</param>
    /// <param name="aiClientTypeEnum">Le type de client AI à utiliser pour le calcul.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Le nombre total de calories estimé.</returns>
    Task<int> AskCalories(List<IngredientDto> ingredients, AiClientType aiClientTypeEnum, CancellationToken cancellationToken = default);

    /// <summary>
    /// Génère un résumé d'une vidéo YouTube à partir des données fournies.
    /// </summary>
    /// <param name="request">Les données JSON de la vidéo YouTube à résumer.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Le résumé généré de la vidéo YouTube.</returns>
    Task<string> GetYoutubeResume(YoutubeSummaryJson request, CancellationToken cancellationToken = default);
}