using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RecettesFamille.Data.EntityModel;
using RecettesFamille.Data.EntityModel.Blocks;
using RecettesFamille.Data.Repository.IRepositories;
using RecettesFamille.Dto.ModelByPage.RecetteBook;
using RecettesFamille.Dto.Models;
using RecettesFamille.Dto.Models.Blocks;

namespace RecettesFamille.Data.Repository.Repositories;

public class RecipeRepository(IMapper mapper, IDbContextFactory<ApplicationDbContext> contextFactory) : IRecipeRepository
{
    public async Task<List<RecipeDto>> GetAll(CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var result = await context.Recipes.ToListAsync(cancellationToken);

        return mapper.Map<List<RecipeDto>>(result);
    }

    public async Task<List<RecipeDto>> GetAll(string tag, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var result = await context.Recipes
            .Where(r => EF.Functions.ILike(r.Tags, $"%{tag}%"))
            .ToListAsync(cancellationToken);

        return mapper.Map<List<RecipeDto>>(result);
    }

    public async Task<List<RecipeDto>> GetAll(string[] tags, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Recipes.AsQueryable();
        foreach (var tag in tags)
        {
            query = query.Where(r => EF.Functions.ILike(r.Tags, $"%{tag}%"));
        }

        var result = await query.ToListAsync(cancellationToken);

        return mapper.Map<List<RecipeDto>>(result);
    }

    public async Task<List<RecipeForListDto>> GetAllLightRecipe(CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var result = await context.Recipes
            .Select(c => new RecipeForListDto
            {
                Id = c.Id,
                Name = c.Name,
                Tags = c.Tags,
                CreatedDate = c.CreatedDate,
                Image = c.BlocksInstructions
                         .Where(c => c is BlockImageEntity)
                         .Select(b => (b as BlockImageEntity).Image)
                         .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<List<RecipeForListDto>> GetAllLightRecipe(string[] tags, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var query = context.Recipes.AsQueryable();
        foreach (var tag in tags)
        {
            query = query.Where(r => EF.Functions.ILike(r.Tags, $"%{tag}%"));
        }

        var result = await query.Select(c => new RecipeForListDto
        {
            Id = c.Id,
            Name = c.Name,
            Tags = c.Tags,
            CreatedDate = c.CreatedDate
        }).ToListAsync(cancellationToken);

        return result;
    }


    public async Task<RecipeDto> GetWithInstructions(int recipeId, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var result = await context.Recipes.Include(s => s.BlocksInstructions)
            .ThenInclude(b => ((BlockIngredientListEntity)b).Ingredients)
            .Where(r => r.Id == recipeId)
            .FirstOrDefaultAsync(cancellationToken);

        return mapper.Map<RecipeDto>(result);
    }

    #region Recipe

    public async Task<RecipeDto> AddRecipe(RecipeDto recipe, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var recipeEntity = mapper.Map<RecipeEntity>(recipe);

        await context.Set<RecipeEntity>().AddAsync(recipeEntity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return mapper.Map<RecipeDto>(recipeEntity);
    }

    public async Task DeleteRecipe(int recipeId, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var element = await context.Recipes.FindAsync([recipeId], cancellationToken);
        if (element != null)
        {
            context.Recipes.Remove(element);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> UpdateRecipe(RecipeDto? recipe, CancellationToken cancellationToken = default)
    {
        if (recipe is null)
            return false;

        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var element = await context.Recipes.FindAsync([recipe.Id], cancellationToken);

        mapper.Map(recipe, element, opt => { opt.AfterMap((src, dest) => dest!.BlocksInstructions = null!); });

        var result = await context.SaveChangesAsync(cancellationToken);
        return result > 0;
    }

    public async Task UpdateFullRecipe(RecipeDto? recipe, CancellationToken cancellationToken = default)
    {
        if (recipe is null)
            return;

        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var element = await context.Recipes.FindAsync([recipe.Id], cancellationToken);

        mapper.Map(recipe, element);

        await context.SaveChangesAsync(cancellationToken);
    }

    #endregion


    #region Blocks

    public async Task<bool> DeleteBlock(int blockId, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var element = await context.Set<BlockBaseEntity>().FindAsync([blockId], cancellationToken);
        if (element is null)
            return false;

        context.Set<BlockBaseEntity>().Remove(element);
        var result = await context.SaveChangesAsync(cancellationToken);

        return result > 0;
    }

    public async Task UpdateBlock(BlockBaseDto? block, CancellationToken cancellationToken = default)
    {
        if (block is null)
            return;

        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var element = await context.Set<BlockBaseEntity>().FindAsync([block.Id], cancellationToken);

        mapper.Map(block, element, opts =>
        {
            opts.AfterMap((src, dest) => dest!.Recipe = null!);
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<BlockBaseDto> AddBlock(BlockBaseDto block, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        BlockBaseEntity blockEntity = mapper.Map<BlockBaseEntity>(block);

        await context.Set<BlockBaseEntity>().AddAsync(blockEntity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return mapper.Map<BlockBaseDto>(blockEntity);
    }

    #endregion

    #region Ingredients

    public async Task<bool> DeleteIngredient(int ingredientId, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var element = await context.Set<IngredientEntity>().FindAsync([ingredientId], cancellationToken);
        if (element is null)
            return false;

        context.Set<IngredientEntity>().Remove(element);
        var result = await context.SaveChangesAsync(cancellationToken);

        return result > 0;
    }

    public async Task UpdateIngredient(IngredientDto block, CancellationToken cancellationToken = default)
    {
        if (block is null)
            return;

        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var element = await context.Set<IngredientEntity>().FindAsync([block.Id], cancellationToken);

        mapper.Map(block, element, opts =>
        {
            opts.AfterMap((src, dest) => dest!.IngredientList = null!);
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IngredientDto> AddIngredient(IngredientDto ingredient, CancellationToken cancellationToken = default)
    {
        if (ingredient is null)
            throw new ArgumentNullException(nameof(ingredient));

        IngredientEntity ingredientEntity = mapper.Map<IngredientEntity>(ingredient);

        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Set<IngredientEntity>().Add(ingredientEntity);
        await context.SaveChangesAsync(cancellationToken);

        return mapper.Map<IngredientDto>(ingredientEntity);
    }

    #endregion
}