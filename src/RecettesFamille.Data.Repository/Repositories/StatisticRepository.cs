using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RecettesFamille.Data.Repository.IRepositories;

namespace RecettesFamille.Data.Repository.Repositories;

public class StatisticRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : IStatisticRepository
{
    public async Task<List<StatisticsViewModel>> GetCountCallByDays(CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var result = await context.AiConsumptions
                             .GroupBy(a => new { a.Date.Date, a.AiModelName })
                             .Select(g => new StatisticsViewModel()
                             {
                                 Date = g.Key.Date,
                                 AiModelName = g.Key.AiModelName,
                                 Calculate = (double)g.Count()
                             })
                             .OrderBy(x => x.Date)
                             .ToListAsync(cancellationToken);
        return result;
    }

    public async Task<List<StatisticsViewModel>> GetAvgTokenByDays(CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var queryAvg = await context.AiConsumptions
                        .GroupBy(a => new { a.Date.Date, a.AiModelName })
                        .Select(g => new StatisticsViewModel()
                        {
                            Date = g.Key.Date,
                            AiModelName = g.Key.AiModelName,
                            Calculate = (double)g.Average(a => a.InputPrice + a.OutputPrice)
                        })
                        .OrderBy(x => x.Date)
                        .ToListAsync(cancellationToken);
        return queryAvg;
    }
}


public class StatisticsViewModel
{
    public DateTime Date { get; set; }
    public string AiModelName { get; set; } = string.Empty;
    public double Calculate { get; set; }
}
