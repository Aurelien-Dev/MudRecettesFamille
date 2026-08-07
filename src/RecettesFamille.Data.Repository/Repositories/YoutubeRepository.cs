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
            .Include(y => y.Categories)
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

    public async Task<bool> UpdateFavorite(int summaryId, bool isFavorite, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var youtubeResumeEntity = await context.YoutubeSummarys.FindAsync([summaryId], cancellationToken);

        if (youtubeResumeEntity == null)
        {
            return false;
        }

        youtubeResumeEntity.IsFavorite = isFavorite;
        context.YoutubeSummarys.Update(youtubeResumeEntity);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UpdateStatus(int summaryId, Dto.Models.SummaryStatus status, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var youtubeResumeEntity = await context.YoutubeSummarys.FindAsync([summaryId], cancellationToken);

        if (youtubeResumeEntity == null)
        {
            return false;
        }

        // Convert DTO enum to Entity enum (they have the same values)
        youtubeResumeEntity.Status = (EntityModel.SummaryStatus)status;
        context.YoutubeSummarys.Update(youtubeResumeEntity);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UpdateCategories(int summaryId, List<int> categoryIds, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var youtubeResumeEntity = await context.YoutubeSummarys
            .Include(y => y.Categories)
            .FirstOrDefaultAsync(y => y.Id == summaryId, cancellationToken);

        if (youtubeResumeEntity == null)
        {
            return false;
        }

        // Load the categories from the database
        var categories = await context.Categories
            .Where(c => categoryIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        // Clear existing categories and add new ones
        youtubeResumeEntity.Categories.Clear();
        foreach (var category in categories)
        {
            youtubeResumeEntity.Categories.Add(category);
        }

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UpdateTitle(int summaryId, string title, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var youtubeResumeEntity = await context.YoutubeSummarys.FindAsync([summaryId], cancellationToken);

        if (youtubeResumeEntity == null)
        {
            return false;
        }

        youtubeResumeEntity.Title = title;
        context.YoutubeSummarys.Update(youtubeResumeEntity);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
