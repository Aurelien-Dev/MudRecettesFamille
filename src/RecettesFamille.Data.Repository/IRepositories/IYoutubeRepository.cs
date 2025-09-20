using RecettesFamille.Data.EntityModel;
using RecettesFamille.Dto.Models;

namespace RecettesFamille.Data.Repository.IRepositories;

/// <summary>
/// Interface pour la gestion des résumés de vidéos YouTube.
/// Fournit des méthodes pour ajouter et récupérer des résumés de vidéos.
/// </summary>
public interface IYoutubeRepository
{
    /// <summary>
    /// Ajoute un nouveau résumé de vidéo YouTube.
    /// </summary>
    /// <param name="youtubeSummary">Le résumé de vidéo à ajouter.</param>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Le résumé ajouté avec son identifiant.</returns>
    Task<YoutubeResumeDto> AddSummary(YoutubeResumeDto youtubeSummary, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Récupère tous les résumés de vidéos YouTube disponibles.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Liste de tous les résumés de vidéos.</returns>
    Task<List<YoutubeResumeDto>> GetAllSummary(CancellationToken cancellationToken = default);
}