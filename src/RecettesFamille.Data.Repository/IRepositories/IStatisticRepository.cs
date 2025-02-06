using RecettesFamille.Data.Repository.Repositories;

namespace RecettesFamille.Data.Repository.IRepositories;

public interface IStatisticRepository
{
    Task<List<StatisticsViewModel>> GetAvgTokenByDays(CancellationToken cancellationToken = default);
    Task<List<StatisticsViewModel>> GetCountCallByDays(CancellationToken cancellationToken = default);
}