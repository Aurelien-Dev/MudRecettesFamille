namespace RecettesFamille.Data.EntityModel.Blocks;
public abstract class BlockBaseEntity
{
    public int? Id { get; set; }
    public int Order { get; set; }
    public bool HalfPage { get; set; } = false;

    public RecipeEntity Recipe { get; set; } = null!;
}
