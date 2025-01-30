using RecettesFamille.Data.EntityModel.Blocks;

namespace RecettesFamille.Data.EntityModel;

public class RecipeEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InformationPreparation { get; set; } = string.Empty;

    public List<BlockBaseEntity> BlocksInstructions { get; set; } = new List<BlockBaseEntity>();
}










