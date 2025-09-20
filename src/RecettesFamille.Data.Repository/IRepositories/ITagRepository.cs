using RecettesFamille.Dto.Models;

namespace RecettesFamille.Data.Repository.IRepositories;

/// <summary>
/// Interface pour la gestion des tags dans l'application.
/// Fournit des méthodes pour récupérer, ajouter, modifier et supprimer des tags.
/// </summary>
public interface ITagRepository
{
    /// <summary>
    /// Récupère tous les tags, visibles ou non.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Liste de tous les tags.</returns>
    Task<List<TagDto>> GetAll(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Récupère uniquement les tags marqués comme visibles.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Liste des tags visibles.</returns>
    Task<List<TagDto>> GetAllVisible(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ajoute un nouveau tag.
    /// </summary>
    /// <param name="tag">Le tag à ajouter.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Le tag ajouté avec son identifiant.</returns>
    Task<TagDto> AddTag(TagDto tag, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ajoute plusieurs tags en une seule opération.
    /// </summary>
    /// <param name="tags">Tableau de tags à ajouter.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>True si l'ajout a réussi pour tous les tags, sinon False.</returns>
    Task<bool> AddTag(TagDto[] tags, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Met à jour un tag existant.
    /// </summary>
    /// <param name="tag">Le tag avec les informations mises à jour.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Une tâche représentant l'opération asynchrone.</returns>
    Task UpdateTag(TagDto tag, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Supprime un tag et retire toutes ses associations aux recettes.
    /// </summary>
    /// <param name="tag">Le tag à supprimer.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>True si la suppression a réussi, sinon False.</returns>
    Task<bool> DeleteTagOnRecipe(TagDto tag, CancellationToken cancellationToken = default);
}
