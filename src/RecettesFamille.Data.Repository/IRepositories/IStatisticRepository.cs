
using RecettesFamille.Data.Repository.Repositories;

namespace RecettesFamille.Data.Repository.IRepositories;

public interface IStatisticRepository
{
    Task<List<StatVM>> GetAvgTokenByDays(CancellationToken cancellationToken = default);
    Task<List<StatVM>> GetCountCallByDays(CancellationToken cancellationToken = default);
}