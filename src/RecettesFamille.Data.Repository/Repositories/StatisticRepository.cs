using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RecettesFamille.Data.Repository.IRepositories;

namespace RecettesFamille.Data.Repository.Repositories;

public class StatisticRepository(IMapper Mapper, ApplicationDbContext Context) : IStatisticRepository
{
    public async Task<List<StatVM>> GetCountCallByDays(CancellationToken cancellationToken = default)
    {
        var result = await Context.AiConsumptions
                             .GroupBy(a => new { a.Date.Date, a.AiModelName })
                             .Select(g => new StatVM()
                             {
                                 Date = g.Key.Date,
                                 AiModelName = g.Key.AiModelName,
                                 Calculate = (double)g.Count()
                             })
                             .OrderBy(x => x.Date)
                             .ToListAsync();

        return result;
    }

    public async Task<List<StatVM>> GetAvgTokenByDays(CancellationToken cancellationToken = default)
    {
        var queryAvg = await Context.AiConsumptions
                        .GroupBy(a => new { a.Date.Date, a.AiModelName })
                        .Select(g => new StatVM()
                        {
                            Date = g.Key.Date,
                            AiModelName = g.Key.AiModelName,
                            Calculate = (double)g.Average(a => a.InputPrice + a.OutputPrice)
                        })
                        .OrderBy(x => x.Date)
                        .ToListAsync();
        return queryAvg;
    }
}


public class StatVM
{
    public DateTime Date { get; set; }
    public string AiModelName { get; set; }
    public double Calculate { get; set; }
}
