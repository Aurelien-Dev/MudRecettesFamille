namespace RecettesFamille.Dto.Models.Blocks;

public abstract class BlockBaseDto
{
    public int? Id { get; set; }
    public int Order { get; set; }
    public bool HalfPage { get; set; } = false;
    public int RecipeId { get; set; }

    public RecipeDto Recipe { get; set; } = null!;
}
