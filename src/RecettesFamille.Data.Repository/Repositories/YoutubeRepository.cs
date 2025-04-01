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
        return mapper.Map<List<YoutubeResumeDto>>(await context.YoutubeSummarys.ToListAsync(cancellationToken));
    }

    public async Task<YoutubeResumeDto> AddSummary(YoutubeResumeDto youtubeSummary, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var recipeEntity = mapper.Map<YoutubeResumeEntity>(youtubeSummary);

        await context.YoutubeSummarys.AddAsync(recipeEntity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return youtubeSummary;
    }
}
