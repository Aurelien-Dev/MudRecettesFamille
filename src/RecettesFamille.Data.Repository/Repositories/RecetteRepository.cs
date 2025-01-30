using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RecettesFamille.Data.EntityModel;
using RecettesFamille.Data.EntityModel.Blocks;
using RecettesFamille.Data.Repository.IRepositories;
using RecettesFamille.Dto.Models;
using RecettesFamille.Dto.Models.Blocks;

namespace RecettesFamille.Data.Repository.Repositories;

public class RecetteRepository(IMapper Mapper, ApplicationDbContext Context) : IRecetteRepository
{

    public async Task<List<RecipeDto>> GetAll(CancellationToken cancellationToken = default)
    {
        var result = await Context.Recettes.Include(c => c.BlocksInstructions).ToListAsync(cancellationToken);

        return Mapper.Map<List<RecipeDto>>(result);
    }

    public async Task<RecipeDto> GetWithInstructions(int recipeId, CancellationToken cancellationToken = default)
    {
        var result = await Context.Recettes.Include(s => s.BlocksInstructions)
                                            .ThenInclude(b => ((BlockIngredientListEntity)b).Ingredients)
                                            .Where(r => r.Id == recipeId)
                                            .FirstOrDefaultAsync();

        return Mapper.Map<RecipeDto>(result);
    }

    #region Recipe
    public async Task AddRecipe(RecipeDto block, CancellationToken cancellationToken = default)
    {
        RecipeEntity blockEntity = Mapper.Map<RecipeEntity>(block);

        await Context.Set<RecipeEntity>().AddAsync(blockEntity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRecipe(int recipeId, CancellationToken cancellationToken = default)
    {
        var element = await Context.Recettes.FindAsync(recipeId, cancellationToken);
        if (element != null)
        {
            Context.Recettes.Remove(element);
            await Context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateRecipe(RecipeDto recipe, CancellationToken cancellationToken = default)
    {
        var element = await Context.Recettes.FindAsync(recipe.Id, cancellationToken);
        if (recipe is null)
            return;

        Mapper.Map(recipe, element);

        await Context.SaveChangesAsync(cancellationToken);
    }
    #endregion


    #region Blocks
    public async Task<bool> DeleteBlock(int blockId, CancellationToken cancellationToken = default)
    {
        var element = await Context.Set<BlockBaseEntity>().FindAsync(blockId, cancellationToken);
        if (element is null)
            return false;

        Context.Set<BlockBaseEntity>().Remove(element);
        var result = await Context.SaveChangesAsync(cancellationToken);

        return result > 0;
    }

    public async Task UpdateBlock(BlockBaseDto block, CancellationToken cancellationToken = default)
    {
        var element = await Context.Set<BlockBaseEntity>().FindAsync(block.Id, cancellationToken);
        if (block is null)
            return;

        Mapper.Map(block, element);

        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<BlockBaseDto> AddBlock(BlockBaseDto block, CancellationToken cancellationToken = default)
    {
        BlockBaseEntity blockEntity = Mapper.Map<BlockBaseEntity>(block);

        await Context.Set<BlockBaseEntity>().AddAsync(blockEntity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);

        return Mapper.Map<BlockBaseDto>(blockEntity);
    }
    #endregion

    #region Ingredients
    public async Task<bool> DeleteIngredient(int ingredientId, CancellationToken cancellationToken = default)
    {
        var element = await Context.Set<IngredientEntity>().FindAsync(ingredientId, cancellationToken);
        if (element is null)
            return false;

        Context.Set<IngredientEntity>().Remove(element);
        var result = await Context.SaveChangesAsync(cancellationToken);

        return result > 0;
    }

    #endregion
}
