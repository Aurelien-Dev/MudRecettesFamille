using RecettesFamille.Data.EntityModel;

namespace RecettesFamille.Managers.AiGenerators
{
    public interface IRecipeConverteBase
    {
        Task<RecipeEntity> Convert(string recipe, CancellationToken cancellationToken = default);
    }
}
