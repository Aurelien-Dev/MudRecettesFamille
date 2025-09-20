using RecettesFamille.Dto.ModelByPage.RecetteBook;
using RecettesFamille.Dto.Models;
using RecettesFamille.Dto.Models.Blocks;

namespace RecettesFamille.Data.Repository.IRepositories;

/// <summary>
/// Interface pour la gestion des recettes dans l'application.
/// Fournit des méthodes pour récupérer, ajouter, modifier et supprimer des recettes et leurs composants.
/// </summary>
public interface IRecipeRepository
{
    /// <summary>
    /// Récupère toutes les recettes.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Liste de toutes les recettes.</returns>
    Task<List<RecipeDto>> GetAll(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Récupère toutes les recettes ayant un tag spécifique.
    /// </summary>
    /// <param name="tag">Le tag à rechercher.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Liste des recettes correspondant au tag.</returns>
    Task<List<RecipeDto>> GetAll(string tag, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Récupère toutes les recettes ayant les tags spécifiés.
    /// </summary>
    /// <param name="tags">Tableau de tags à rechercher.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Liste des recettes correspondant aux tags.</returns>
    Task<List<RecipeDto>> GetAll(string[] tags, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Récupère une version légère de toutes les recettes (sans instructions détaillées).
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Liste de recettes en format léger.</returns>
    Task<List<RecipeForListDto>> GetAllLightRecipe(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Récupère une version légère des recettes ayant les tags spécifiés.
    /// </summary>
    /// <param name="tags">Tableau de tags à rechercher.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Liste de recettes en format léger correspondant aux tags.</returns>
    Task<List<RecipeForListDto>> GetAllLightRecipe(string[] tags, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Récupère une version légère des recettes spécifiées par leurs identifiants.
    /// </summary>
    /// <param name="ids">Tableau d'identifiants de recettes.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Liste de recettes en format léger correspondant aux identifiants.</returns>
    Task<List<RecipeForListDto>> GetAllLightRecipe(int[] ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère une version légère d'une recette spécifique.
    /// </summary>
    /// <param name="recipeId">Identifiant de la recette.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>La recette en format léger.</returns>
    Task<RecipeForListDto> GetLightRecipe(int recipeId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Récupère une recette complète avec ses instructions.
    /// </summary>
    /// <param name="recipeId">Identifiant de la recette.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>La recette complète avec instructions.</returns>
    Task<RecipeDto> GetWithInstructions(int recipeId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Récupère le texte brut d'une recette.
    /// </summary>
    /// <param name="recipeId">Identifiant de la recette.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Le texte brut de la recette.</returns>
    Task<string> GetRawRecipe(int recipeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajoute une nouvelle recette.
    /// </summary>
    /// <param name="recipe">La recette à ajouter.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>La recette ajoutée avec son identifiant.</returns>
    Task<RecipeDto> AddRecipe(RecipeDto recipe, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Supprime une recette.
    /// </summary>
    /// <param name="recipeId">Identifiant de la recette à supprimer.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Une tâche représentant l'opération asynchrone.</returns>
    Task DeleteRecipe(int recipeId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Met à jour les informations de base d'une recette.
    /// </summary>
    /// <param name="recipe">La recette avec les informations mises à jour.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>True si la mise à jour a réussi, sinon False.</returns>
    Task<bool> UpdateRecipe(RecipeDto? recipe, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Met à jour une recette complète avec tous ses composants.
    /// </summary>
    /// <param name="recipe">La recette avec les informations mises à jour.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Une tâche représentant l'opération asynchrone.</returns>
    Task UpdateFullRecipe(RecipeDto? recipe, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajoute un bloc d'instructions à une recette.
    /// </summary>
    /// <param name="block">Le bloc à ajouter.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Le bloc ajouté avec son identifiant.</returns>
    Task<BlockBaseDto> AddBlock(BlockBaseDto block, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Met à jour un bloc d'instructions.
    /// </summary>
    /// <param name="block">Le bloc avec les informations mises à jour.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Une tâche représentant l'opération asynchrone.</returns>
    Task UpdateBlock(BlockBaseDto? block, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Supprime un bloc d'instructions.
    /// </summary>
    /// <param name="blockId">Identifiant du bloc à supprimer.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>True si la suppression a réussi, sinon False.</returns>
    Task<bool> DeleteBlock(int blockId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajoute un ingrédient à une recette.
    /// </summary>
    /// <param name="ingredient">L'ingrédient à ajouter.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>L'ingrédient ajouté avec son identifiant.</returns>
    Task<IngredientDto> AddIngredient(IngredientDto ingredient, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Supprime un ingrédient d'une recette.
    /// </summary>
    /// <param name="ingredientId">Identifiant de l'ingrédient à supprimer.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>True si la suppression a réussi, sinon False.</returns>
    Task<bool> DeleteIngredient(int ingredientId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Met à jour un ingrédient.
    /// </summary>
    /// <param name="block">L'ingrédient avec les informations mises à jour.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Une tâche représentant l'opération asynchrone.</returns>
    Task UpdateIngredient(IngredientDto block, CancellationToken cancellationToken = default);
}