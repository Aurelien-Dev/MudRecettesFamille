namespace RecettesFamille.Data.EntityModel.Blocks;
public abstract class BlockBaseEntity
{
    public int? Id { get; set; }
    public int Order { get; set; }
    public bool HalfPage { get; set; } = false;
    public int RecipeId { get; set; }

    public RecipeEntity Recipe { get; set; } = null!;
}
