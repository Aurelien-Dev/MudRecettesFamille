namespace RecettesFamille.Data.EntityModel.RecipeSubEntity;
public class BlockIngredientListEntity : BlockBase
{
    public List<IngredientEntity> Ingredients { get; set; }
}


public record class IngredientEntity()
{
    public int Id { get; set; }
    public int Order { get; set; }
    public string Name { get; set; }
    public string Quantity { get; set; } = string.Empty;
    public BlockIngredientListEntity IngredientList { get; set; }
}