using RecettesFamille.Dto.Models;
using RecettesFamille.Managers.AiGenerators.Models;

namespace RecettesFamille.Managers.AiGenerators
{
    public interface IAiManager
    {
        Task<RecipeDto> ConvertRecipe(string recipe, AiClientTypeEnum aiClientTypeEnum, CancellationToken cancellationToken = default);
        Task<string> AskImage(string recipeName, CancellationToken cancellationToken = default);
    }
}
