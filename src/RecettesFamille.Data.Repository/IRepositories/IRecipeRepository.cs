using RecettesFamille.Dto.ModelByPage.RecetteBook;
using RecettesFamille.Dto.Models;
using RecettesFamille.Dto.Models.Blocks;

namespace RecettesFamille.Data.Repository.IRepositories;

public interface IRecipeRepository
{
    Task<List<RecipeDto>> GetAll(CancellationToken cancellationToken = default);
    Task<List<RecipeDto>> GetAll(string tag, CancellationToken cancellationToken = default);
    Task<List<RecipeDto>> GetAll(string[] tags, CancellationToken cancellationToken = default);
    Task<List<RecipeForListDto>> GetAllLightRecipe(CancellationToken cancellationToken = default);
    Task<List<RecipeForListDto>> GetAllLightRecipe(string[] tags, CancellationToken cancellationToken = default);

    Task<RecipeDto> GetWithInstructions(int recipeId, CancellationToken cancellationToken = default);

    Task DeleteRecipe(int recipeId, CancellationToken cancellationToken = default);
    Task<RecipeDto> AddRecipe(RecipeDto block, CancellationToken cancellationToken = default);
    Task<bool> UpdateRecipe(RecipeDto? recipe, CancellationToken cancellationToken = default);

    Task<BlockBaseDto> AddBlock(BlockBaseDto block, CancellationToken cancellationToken = default);
    Task UpdateBlock(BlockBaseDto? block, CancellationToken cancellationToken = default);
    Task<bool> DeleteBlock(int blockId, CancellationToken cancellationToken = default);

    Task<bool> DeleteIngredient(int ingredientId, CancellationToken cancellationToken = default);
    Task UpdateFullRecipe(RecipeDto? recipe, CancellationToken cancellationToken = default);
}