using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RecettesFamille.Data.EntityModel;
using RecettesFamille.Data.Repository.IRepositories;
using RecettesFamille.Dto.Models.Blocks;

namespace RecettesFamille.Data.Repository.Repositories;

public class TagRepository(IMapper Mapper, IDbContextFactory<ApplicationDbContext> contextFactory) : ITagRepository
{
    public async Task UpdateBlock(TagDto tag, CancellationToken cancellationToken = default)
    {
        var context = await contextFactory.CreateDbContextAsync();
        var element = await context.Set<TagEntity>().FindAsync(tag.Id, cancellationToken);
        if (tag is null)
            return;

        Mapper.Map(tag, element);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<BlockBaseDto> AddTag(TagDto tag, CancellationToken cancellationToken = default)
    {
        var context = await contextFactory.CreateDbContextAsync();
        TagEntity tagEntity = Mapper.Map<TagEntity>(tag);

        await context.Set<TagEntity>().AddAsync(tagEntity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Mapper.Map<BlockBaseDto>(tagEntity);
    }

    public async Task<BlockBaseDto> AddTag(TagDto[] tags, CancellationToken cancellationToken = default)
    {
        var context = await contextFactory.CreateDbContextAsync();

        // Exclude existing tags
        var existingTagNames = await context.Tags.Select(t => t.TagName).ToListAsync(cancellationToken);
        var newTags = tags.Where(t => !existingTagNames.Contains(t.TagName)).ToArray();

        TagEntity[] tagEntity = Mapper.Map<TagEntity[]>(newTags);

        await context.Set<TagEntity>().AddRangeAsync(tagEntity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Mapper.Map<BlockBaseDto>(tagEntity);
    }
}
