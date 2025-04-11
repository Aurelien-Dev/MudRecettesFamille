using RecettesFamille.Dto.Models;
using RecettesFamille.Dto.Models.Blocks;
using RecettesFamille.Managers.AiGenerators.Models;

namespace RecettesFamille.Managers.AiGenerators
{
    public interface IAiManager
    {
        Task<RecipeDto> ConvertRecipe(string recipe, AiClientType aiClientTypeEnum, CancellationToken cancellationToken = default);
        Task<string> AskImage(string recipeName, CancellationToken cancellationToken = default);
        Task<int> AskCalories(List<IngredientDto> ingredients, AiClientType aiClientTypeEnum, CancellationToken cancellationToken = default);

        Task<string> GetYoutubeResume(YoutubeSummaryJson request, CancellationToken cancellationToken = default);
    }
}
