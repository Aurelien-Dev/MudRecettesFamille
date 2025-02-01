using RecettesFamille.Dto.Models;

namespace RecettesFamille.Managers.AiGenerators
{
    public interface IIaManagerBase
    {
        Task<RecipeDto> ConvertRecipe(string recipe, CancellationToken cancellationToken = default);
        Task<string> AskImage(string recipeName, CancellationToken cancellationToken = default);
    }
}
