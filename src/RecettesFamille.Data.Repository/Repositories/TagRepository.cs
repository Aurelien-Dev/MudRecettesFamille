﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RecettesFamille.Data.EntityModel;
using RecettesFamille.Data.Repository.IRepositories;
using RecettesFamille.Dto.Models;

namespace RecettesFamille.Data.Repository.Repositories;

public class TagRepository(IMapper mapper, IDbContextFactory<ApplicationDbContext> contextFactory) : ITagRepository
{
    public async Task<List<TagDto>> GetAll(CancellationToken cancellationToken = default)
    {
        var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var result = await context.Tags.ToListAsync(cancellationToken);

        return mapper.Map<List<TagDto>>(result);
    }

    public async Task<List<TagDto>> GetAllVisible(CancellationToken cancellationToken = default)
    {
        var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var result = await context.Tags.Where(c => c.IsVisible).ToListAsync(cancellationToken);

        return mapper.Map<List<TagDto>>(result);
    }

    public async Task UpdateTag(TagDto tag, CancellationToken cancellationToken = default)
    {
        var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var element = await context.Set<TagEntity>().FindAsync(tag.Id, cancellationToken);
        
        mapper.Map(tag, element);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TagDto> AddTag(TagDto tag, CancellationToken cancellationToken = default)
    {
        var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var tagEntity = mapper.Map<TagEntity>(tag);

        await context.Set<TagEntity>().AddAsync(tagEntity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return mapper.Map<TagDto>(tagEntity);
    }

    public async Task<bool> AddTag(TagDto[] tags, CancellationToken cancellationToken = default)
    {
        var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // Exclude existing tags
        var existingTagNames = await context.Tags.Select(t => t.TagName).ToListAsync(cancellationToken);
        var newTags = tags.Where(t => !existingTagNames.Contains(t.TagName)).ToArray();

        var tagEntity = mapper.Map<TagEntity[]>(newTags);

        await context.Set<TagEntity>().AddRangeAsync(tagEntity, cancellationToken);
        var result = await context.SaveChangesAsync(cancellationToken);

        return result > 0;
    }
}