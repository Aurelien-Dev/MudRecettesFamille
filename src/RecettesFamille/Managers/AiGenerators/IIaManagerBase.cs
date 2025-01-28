using RecettesFamille.Data.EntityModel;

namespace RecettesFamille.Managers.AiGenerators
{
    public interface IIaManagerBase
    {
        Task<RecipeEntity> ConvertRecipe(string recipe, CancellationToken cancellationToken = default);
        Task<string> AskImage(CancellationToken cancellationToken = default);
    }
}
