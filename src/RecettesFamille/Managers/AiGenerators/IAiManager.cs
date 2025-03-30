using RecettesFamille.Dto.Models;
using RecettesFamille.Managers.AiGenerators.Models;

namespace RecettesFamille.Managers.AiGenerators
{
    public interface IAiManager
    {
        Task<RecipeDto> ConvertRecipe(string recipe, AiClientType aiClientTypeEnum, CancellationToken cancellationToken = default);
        Task<string> AskImage(string recipeName, CancellationToken cancellationToken = default);


        Task<string> GetYoutubeResume(string transcript, CancellationToken cancellationToken = default);
    }
}
