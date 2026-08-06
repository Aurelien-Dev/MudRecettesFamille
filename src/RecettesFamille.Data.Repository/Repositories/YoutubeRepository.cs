using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RecettesFamille.Data.EntityModel;
using RecettesFamille.Data.Repository.IRepositories;
using RecettesFamille.Dto.Models;

namespace RecettesFamille.Data.Repository.Repositories;

public class YoutubeRepository(IMapper mapper, IDbContextFactory<ApplicationDbContext> contextFactory) : IYoutubeRepository
{
    public async Task<List<YoutubeResumeDto>> GetAllSummary(CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var summaries = await context.YoutubeSummarys
            .Include(y => y.Travel)
            .ToListAsync(cancellationToken);
        return mapper.Map<List<YoutubeResumeDto>>(summaries);
    }

    public async Task<YoutubeResumeDto> AddSummary(YoutubeResumeDto youtubeSummary, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var youtubeResumeEntity = mapper.Map<YoutubeResumeEntity>(youtubeSummary);

        await context.YoutubeSummarys.AddAsync(youtubeResumeEntity, cancellationToken);
        _ = await context.SaveChangesAsync(cancellationToken);

        return youtubeSummary;
    }

    public async Task<bool> DeleteSummary(int id, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var youtubeResumeEntity = await context.YoutubeSummarys.FindAsync([id], cancellationToken);

        if (youtubeResumeEntity == null)
        {
            return false;
        }

        context.YoutubeSummarys.Remove(youtubeResumeEntity);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UpdateSummaryTravel(int summaryId, int? travelId, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var youtubeResumeEntity = await context.YoutubeSummarys.FindAsync([summaryId], cancellationToken);

        if (youtubeResumeEntity == null)
        {
            return false;
        }

        youtubeResumeEntity.TravelId = travelId;
        context.YoutubeSummarys.Update(youtubeResumeEntity);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
