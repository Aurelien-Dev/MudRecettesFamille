using RecettesFamille.Data.EntityModel.RecipeSubEntity;

namespace RecettesFamille.Data.EntityModel;

public class RecipeEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InformationPreparation { get; set; } = string.Empty;

    public List<BlockBase> BlocksInstructions { get; set; } = new List<BlockBase>();
}










