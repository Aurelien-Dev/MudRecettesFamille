using RecettesFamille.Data.EntityModel.Blocks;

namespace RecettesFamille.Data.EntityModel;

public class RecipeEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InformationPreparation { get; set; } = string.Empty;
    public int PrepTime { get; set; }
    public int CookingTime { get; set; }
    public int Portion { get; set; }
    public string Tags { get; set; }

    public List<BlockBaseEntity> BlocksInstructions { get; set; } = new List<BlockBaseEntity>();
}










