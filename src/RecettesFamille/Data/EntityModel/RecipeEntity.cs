using RecettesFamille.Data.EntityModel.RecipeSubEntity;

namespace RecettesFamille.Data.EntityModel;

public class RecipeEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string InformationPreparation { get; set; }

    public List<BlockBase> BlocksInstructions { get; set; }
}










