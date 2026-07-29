using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecettesFamille.Data.EntityModel;
using RecettesFamille.Data.EntityModel.Blocks;
using RecettesFamille.Data.Repository.IRepositories;
using RecettesFamille.Dto.ModelByPage.RecetteBook;
using RecettesFamille.Dto.Models;
using RecettesFamille.Dto.Models.Blocks;
using RecettesFamille.Rag.Services;
using System.Text;

namespace RecettesFamille.Data.Repository.Repositories;

public class RecipeRepository(
    IMapper mapper,
    IDbContextFactory<ApplicationDbContext> contextFactory,
    RagIngestionService ragIngestion,
    ILogger<RecipeRepository> logger) : IRecipeRepository
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

    public async Task<List<RecipeForListDto>> GetAllLightRecipe(string userEmail, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var user = await context.Users.SingleOrDefaultAsync(u => u.Email == userEmail);
        if (user is null)
            return [];

        var result = await context.Recipes.Include(s => s.FavoritedByUsers)
            .Select(c => new RecipeForListDto
            {
                Id = c.Id,
                Name = c.Name,
                Tags = c.Tags,
                CreatedDate = c.CreatedDate,
                Image = c.BlocksInstructions
                         .Where(c => c is BlockImageEntity)
                         .Select(b => (b as BlockImageEntity).Image)
                         .FirstOrDefault(),
                IsFavorited = c.FavoritedByUsers.Any(u => u.Id == user.Id)
            })
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<List<RecipeForListDto>> GetAllFavoritesLightRecipe(string userEmail, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var user = await context.Users.SingleOrDefaultAsync(u => u.Email == userEmail);
        if (user is null)
            return [];

        var result = await context.Recipes.Include(s => s.FavoritedByUsers)
            .Select(c => new RecipeForListDto
            {
                Id = c.Id,
                Name = c.Name,
                Tags = c.Tags,
                CreatedDate = c.CreatedDate,
                Image = c.BlocksInstructions
                         .Where(c => c is BlockImageEntity)
                         .Select(b => (b as BlockImageEntity).Image)
                         .FirstOrDefault(),
                IsFavorited = c.FavoritedByUsers.Any(u => u.Id == user.Id)
            }).Where(r => r.IsFavorited)
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<List<RecipeForListDto>> GetAllLightRecipe(int[] ids, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var result = await context.Recipes.Where(c => ids.Contains(c.Id))
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


    public async Task<RecipeForListDto> GetLightRecipe(int recipeId, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var result = await context.Recipes.Where(c => c.Id == recipeId)
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
            .FirstOrDefaultAsync(cancellationToken);

        return result;
    }


    public async Task<string> GetRawRecipe(int recipeId, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var result = await context.Recipes.Include(s => s.BlocksInstructions)
            .ThenInclude(b => ((BlockIngredientListEntity)b).Ingredients)
            .Where(r => r.Id == recipeId)
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
            return string.Empty;

        StringBuilder builder = new StringBuilder();

        builder.AppendLine($"Title : {result.Name}");
        builder.AppendLine();
        builder.AppendLine($"Temps de préparation : {result.PrepTime} min, Cuisson : {result.CookingTime} min, Repos : {result.RestTime} min");
        builder.AppendLine($"Portions : {result.Portion}");
        if (!string.IsNullOrWhiteSpace(result.Tags))
        {
            builder.AppendLine($"Tags : {result.Tags}");
        }

        foreach (var item in result.BlocksInstructions.OfType<BlockIngredientListEntity>())
        {
            var ingredientList = item.Ingredients.Select(s => $"{s.Name}:{s.Quantity}").ToList();
            builder.AppendLine("---- Ingredient List");
            builder.AppendLine(string.Join(Environment.NewLine, ingredientList));
        }

        builder.AppendLine();
        builder.AppendLine("---- Instructions");
        foreach (var item in result.BlocksInstructions.OfType<BlockInstructionEntity>())
        {
            builder.AppendLine(item.Instruction);
        }

        return builder.ToString();
    }


    #region Recipe

    public async Task<RecipeDto> AddRecipe(RecipeDto recipe, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var recipeEntity = mapper.Map<RecipeEntity>(recipe);

        await context.Set<RecipeEntity>().AddAsync(recipeEntity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // RAG ingestion hook: ingest new recipe into vector store
        try
        {
            await ragIngestion.IngestSingleAsync(
                recipeEntity.Id,
                recipeEntity.GetSearchableContent(),
                recipeEntity.GetCategory(),
                "recipe");
            logger.LogInformation("RAG: Ingested recipe {RecipeId} '{RecipeName}'", recipeEntity.Id, recipeEntity.Name);
        }
        catch (Exception ex)
        {
            // Lenient failure policy: log error but don't fail the recipe creation
            logger.LogError(ex, "RAG: Failed to ingest recipe {RecipeId} '{RecipeName}'", recipeEntity.Id, recipeEntity.Name);
        }

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

            // RAG ingestion hook: remove recipe from vector store
            try
            {
                await ragIngestion.DeleteSingleAsync(recipeId, "recipe");
                logger.LogInformation("RAG: Deleted recipe {RecipeId}", recipeId);
            }
            catch (Exception ex)
            {
                // Lenient failure policy: log error but don't fail the recipe deletion
                logger.LogError(ex, "RAG: Failed to delete recipe {RecipeId}", recipeId);
            }
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

        // RAG ingestion hook: update recipe in vector store
        if (result > 0 && element != null)
        {
            try
            {
                await ragIngestion.UpdateSingleAsync(
                    element.Id,
                    element.GetSearchableContent(),
                    element.GetCategory(),
                    "recipe");
                logger.LogInformation("RAG: Updated recipe {RecipeId} '{RecipeName}'", element.Id, element.Name);
            }
            catch (Exception ex)
            {
                // Lenient failure policy: log error but don't fail the recipe update
                logger.LogError(ex, "RAG: Failed to update recipe {RecipeId} '{RecipeName}'", element.Id, element.Name);
            }
        }

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

        // RAG ingestion hook: update recipe in vector store
        if (element != null)
        {
            try
            {
                await ragIngestion.UpdateSingleAsync(
                    element.Id,
                    element.GetSearchableContent(),
                    element.GetCategory(),
                    "recipe");
                logger.LogInformation("RAG: Updated full recipe {RecipeId} '{RecipeName}'", element.Id, element.Name);
            }
            catch (Exception ex)
            {
                // Lenient failure policy: log error but don't fail the recipe update
                logger.LogError(ex, "RAG: Failed to update full recipe {RecipeId} '{RecipeName}'", element.Id, element.Name);
            }
        }
    }

    public async Task<bool> AddUserToFavoriteds(int recipeId, string userEmail, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var user = await context.Users.SingleAsync(s => s.UserName == userEmail);
        if (user == null)
            return false;

        var recipe = await context.Recipes.Include(r => r.FavoritedByUsers).FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);
        if (recipe == null || recipe.FavoritedByUsers.Any(u => u.Id == user.Id))
            return false;

        recipe.FavoritedByUsers.Add(user);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteUserToFavoriteds(int recipeId, string userEmail, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var recipe = await context.Recipes.Include(r => r.FavoritedByUsers).FirstOrDefaultAsync(r => r.Id == recipeId, cancellationToken);
        var user = recipe.FavoritedByUsers.SingleOrDefault(u => u.UserName == userEmail);
        if (recipe == null || user == null)
            return false;

        recipe.FavoritedByUsers.Remove(user);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }


    #endregion

    #region Blocks

    public async Task<bool> DeleteBlock(int blockId, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var element = await context.Set<BlockBaseEntity>().FindAsync([blockId], cancellationToken);
        if (element is null)
            return false;

        var recipeId = element.RecipeId;
        context.Set<BlockBaseEntity>().Remove(element);
        var result = await context.SaveChangesAsync(cancellationToken);

        // RAG ingestion hook: update recipe after block deletion
        if (result > 0)
        {
            await UpdateRecipeVectorAsync(context, recipeId);
        }

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

        var recipeId = element?.RecipeId ?? 0;
        await context.SaveChangesAsync(cancellationToken);

        // RAG ingestion hook: update recipe after block update
        if (recipeId > 0)
        {
            await UpdateRecipeVectorAsync(context, recipeId);
        }
    }

    public async Task<BlockBaseDto> AddBlock(BlockBaseDto block, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        BlockBaseEntity blockEntity = mapper.Map<BlockBaseEntity>(block);

        await context.Set<BlockBaseEntity>().AddAsync(blockEntity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // RAG ingestion hook: update recipe after block addition
        await UpdateRecipeVectorAsync(context, blockEntity.RecipeId);

        return mapper.Map<BlockBaseDto>(blockEntity);
    }

    #endregion

    #region Ingredients

    public async Task<bool> DeleteIngredient(int ingredientId, CancellationToken cancellationToken = default)
    {
        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var element = await context.Set<IngredientEntity>()
            .Include(i => i.IngredientList)
            .FirstOrDefaultAsync(i => i.Id == ingredientId, cancellationToken);
        if (element is null)
            return false;

        var recipeId = element.IngredientList?.RecipeId ?? 0;
        context.Set<IngredientEntity>().Remove(element);
        var result = await context.SaveChangesAsync(cancellationToken);

        // RAG ingestion hook: update recipe after ingredient deletion
        if (result > 0 && recipeId > 0)
        {
            await UpdateRecipeVectorAsync(context, recipeId);
        }

        return result > 0;
    }

    public async Task UpdateIngredient(IngredientDto block, CancellationToken cancellationToken = default)
    {
        if (block is null)
            return;

        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var element = await context.Set<IngredientEntity>()
            .Include(i => i.IngredientList)
            .FirstOrDefaultAsync(i => i.Id == block.Id, cancellationToken);

        mapper.Map(block, element, opts =>
        {
            opts.AfterMap((src, dest) => dest!.IngredientList = null!);
        });

        var recipeId = element?.IngredientList?.RecipeId ?? 0;
        await context.SaveChangesAsync(cancellationToken);

        // RAG ingestion hook: update recipe after ingredient update
        if (recipeId > 0)
        {
            await UpdateRecipeVectorAsync(context, recipeId);
        }
    }

    public async Task<IngredientDto> AddIngredient(IngredientDto ingredient, CancellationToken cancellationToken = default)
    {
        if (ingredient is null)
            throw new ArgumentNullException(nameof(ingredient));

        IngredientEntity ingredientEntity = mapper.Map<IngredientEntity>(ingredient);

        using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Set<IngredientEntity>().Add(ingredientEntity);
        await context.SaveChangesAsync(cancellationToken);

        // Load the ingredient list to get recipe ID
        var ingredientWithList = await context.Set<IngredientEntity>()
            .Include(i => i.IngredientList)
            .FirstOrDefaultAsync(i => i.Id == ingredientEntity.Id, cancellationToken);

        var recipeId = ingredientWithList?.IngredientList?.RecipeId ?? 0;
        if (recipeId > 0)
        {
            await UpdateRecipeVectorAsync(context, recipeId);
        }

        return mapper.Map<IngredientDto>(ingredientEntity);
    }

    #endregion

    /// <summary>
    /// Helper method to update recipe vectors after content changes (blocks, ingredients)
    /// </summary>
    private async Task UpdateRecipeVectorAsync(ApplicationDbContext context, int recipeId)
    {
        try
        {
            var recipe = await context.Recipes
                .Include(r => r.BlocksInstructions)
                .ThenInclude(b => (b as BlockIngredientListEntity)!.Ingredients)
                .FirstOrDefaultAsync(r => r.Id == recipeId);

            if (recipe != null)
            {
                await ragIngestion.UpdateSingleAsync(
                    recipe.Id,
                    recipe.GetSearchableContent(),
                    recipe.GetCategory(),
                    "recipe");
                logger.LogInformation("RAG: Updated recipe {RecipeId} after content change", recipeId);
            }
        }
        catch (Exception ex)
        {
            // Lenient failure policy: log error but don't fail the operation
            logger.LogError(ex, "RAG: Failed to update recipe {RecipeId} after content change", recipeId);
        }
    }
}