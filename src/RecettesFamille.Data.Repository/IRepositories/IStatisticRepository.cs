using RecettesFamille.Data.Repository.Repositories;

namespace RecettesFamille.Data.Repository.IRepositories;

/// <summary>
/// Interface pour la gestion des statistiques de l'application.
/// Fournit des méthodes pour récupérer des données statistiques liées à l'utilisation des services AI.
/// </summary>
public interface IStatisticRepository
{
    /// <summary>
    /// Récupère la moyenne des tokens utilisés par jour pour les services AI.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Liste de statistiques montrant la moyenne des tokens par jour.</returns>
    Task<List<StatisticsViewModel>> GetAvgTokenByDays(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Récupère le nombre d'appels aux services AI par jour.
    /// </summary>
    /// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
    /// <returns>Liste de statistiques montrant le nombre d'appels par jour.</returns>
    Task<List<StatisticsViewModel>> GetCountCallByDays(CancellationToken cancellationToken = default);
}