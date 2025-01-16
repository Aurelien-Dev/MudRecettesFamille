using MudBlazor;
using System.Text.Json.Serialization;

namespace RecettesFamille.Data.EntityModel.RecipeSubEntity;
public class BlockIngredientListEntity : BlockBase
{
    public string Name { get; set; } = "Ingrédients";
    public List<IngredientEntity> Ingredients { get; set; } = new List<IngredientEntity>();
}


public record class IngredientEntity()
{
    public int? Id { get; set; }
    public int Order { get; set; }
    public string Name { get; set; }
    public string Quantity { get; set; } = string.Empty;
    public int IngredientListId { get; set; }

    [JsonIgnore]
    public BlockIngredientListEntity IngredientList { get; set; }
}