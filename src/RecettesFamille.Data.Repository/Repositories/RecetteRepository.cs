using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RecettesFamille.Data.EntityModel;
using RecettesFamille.Data.EntityModel.Blocks;
using RecettesFamille.Data.Repository.IRepositories;
using RecettesFamille.Dto.Models;
using RecettesFamille.Dto.Models.Blocks;

namespace RecettesFamille.Data.Repository.Repositories;

public class RecetteRepository(IMapper Mapper, IDbContextFactory<ApplicationDbContext> contextFactory) : IRecetteRepository
{
    public async Task<List<RecipeDto>> GetAll(CancellationToken cancellationToken = default)
    {
        var context = await contextFactory.CreateDbContextAsync();

        var result = await context.Recipes.ToListAsync(cancellationToken);

        return Mapper.Map<List<RecipeDto>>(result);
    }

    public async Task<List<RecipeDto>> GetAllByTag(string tag, CancellationToken cancellationToken = default)
    {
        var context = await contextFactory.CreateDbContextAsync();

        var result = await context.Recipes
                                  .Where(r => EF.Functions.ILike(r.Tags, $"%{tag}%"))
                                  .ToListAsync(cancellationToken);

        return Mapper.Map<List<RecipeDto>>(result);
    }
    public async Task<List<RecipeDto>> GetAllByTag(string[] tags, CancellationToken cancellationToken = default)
    {
        var context = await contextFactory.CreateDbContextAsync();

        var query = context.Recipes.AsQueryable();
        foreach (var tag in tags)
        {
            query = query.Where(r => EF.Functions.ILike(r.Tags, $"%{tag}%"));
        }

        var result = await query.ToListAsync(cancellationToken);

        return Mapper.Map<List<RecipeDto>>(result);
    }

    public async Task<RecipeDto> GetWithInstructions(int recipeId, CancellationToken cancellationToken = default)
    {
        var context = await contextFactory.CreateDbContextAsync();

        var result = await context.Recipes.Include(s => s.BlocksInstructions)
                                            .ThenInclude(b => ((BlockIngredientListEntity)b).Ingredients)
                                            .Where(r => r.Id == recipeId)
                                            .FirstOrDefaultAsync();

        return Mapper.Map<RecipeDto>(result);
    }

    #region Recipe
    public async Task<RecipeDto> AddRecipe(RecipeDto block, CancellationToken cancellationToken = default)
    {
        var context = await contextFactory.CreateDbContextAsync();
        RecipeEntity blockEntity = Mapper.Map<RecipeEntity>(block);

        await context.Set<RecipeEntity>().AddAsync(blockEntity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Mapper.Map<RecipeDto>(blockEntity);
    }

    public async Task DeleteRecipe(int recipeId, CancellationToken cancellationToken = default)
    {
        var context = await contextFactory.CreateDbContextAsync();

        var element = await context.Recipes.FindAsync(recipeId, cancellationToken);
        if (element != null)
        {
            context.Recipes.Remove(element);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateRecipe(RecipeDto recipe, CancellationToken cancellationToken = default)
    {
        var context = await contextFactory.CreateDbContextAsync();
        var element = await context.Recipes.FindAsync(recipe.Id, cancellationToken);
        if (recipe is null)
            return;

        Mapper.Map(recipe, element);

        await context.SaveChangesAsync(cancellationToken);
    }
    #endregion


    #region Blocks
    public async Task<bool> DeleteBlock(int blockId, CancellationToken cancellationToken = default)
    {
        var context = await contextFactory.CreateDbContextAsync();
        var element = await context.Set<BlockBaseEntity>().FindAsync(blockId, cancellationToken);
        if (element is null)
            return false;

        context.Set<BlockBaseEntity>().Remove(element);
        var result = await context.SaveChangesAsync(cancellationToken);

        return result > 0;
    }

    public async Task UpdateBlock(BlockBaseDto block, CancellationToken cancellationToken = default)
    {
        var context = await contextFactory.CreateDbContextAsync();
        var element = await context.Set<BlockBaseEntity>().FindAsync(block.Id, cancellationToken);
        if (block is null)
            return;

        Mapper.Map(block, element);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<BlockBaseDto> AddBlock(BlockBaseDto block, CancellationToken cancellationToken = default)
    {
        var context = await contextFactory.CreateDbContextAsync();
        BlockBaseEntity blockEntity = Mapper.Map<BlockBaseEntity>(block);

        await context.Set<BlockBaseEntity>().AddAsync(blockEntity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Mapper.Map<BlockBaseDto>(blockEntity);
    }
    #endregion

    #region Ingredients
    public async Task<bool> DeleteIngredient(int ingredientId, CancellationToken cancellationToken = default)
    {
        var context = await contextFactory.CreateDbContextAsync();
        var element = await context.Set<IngredientEntity>().FindAsync(ingredientId, cancellationToken);
        if (element is null)
            return false;

        context.Set<IngredientEntity>().Remove(element);
        var result = await context.SaveChangesAsync(cancellationToken);

        return result > 0;
    }

    #endregion
}
