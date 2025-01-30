using RecettesFamille.Dto.Models;
using RecettesFamille.Dto.Models.Blocks;

namespace RecettesFamille.Data.Repository.IRepositories;

public interface IRecetteRepository
{
    Task DeleteRecipe(int recipeId, CancellationToken cancellationToken = default);
    Task<bool> DeleteBlock(int blockId, CancellationToken cancellationToken = default);
    Task<List<RecipeDto>> GetAll(CancellationToken cancellationToken = default);
    Task<RecipeDto> GetWithInstructions(int recipeId, CancellationToken cancellationToken = default);
    Task UpdateRecipe(RecipeDto recipe, CancellationToken cancellationToken = default);
    Task UpdateBlock(BlockBaseDto block, CancellationToken cancellationToken = default);
    Task<BlockBaseDto> AddBlock(BlockBaseDto block, CancellationToken cancellationToken = default);
    Task<bool> DeleteIngredient(int ingredientId, CancellationToken cancellationToken = default);
    Task AddRecipe(RecipeDto block, CancellationToken cancellationToken = default);
}