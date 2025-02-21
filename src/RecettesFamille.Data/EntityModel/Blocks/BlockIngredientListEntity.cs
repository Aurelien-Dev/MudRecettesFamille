using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RecettesFamille.Data.EntityModel.Blocks;
public class BlockIngredientListEntity : BlockBaseEntity
{
    [MaxLength(30)]
    public string Name { get; set; } = "Ingrédients";
    public List<IngredientEntity> Ingredients { get; set; } = [];

    public BlockIngredientListEntity()
    {
        HalfPage = true;
    }
}


public class IngredientEntity()
{
    public int? Id { get; set; }
    public int Order { get; set; }
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(40)]
    public string Quantity { get; set; } = string.Empty;
    public int IngredientListId { get; set; }

    [JsonIgnore]
    public BlockIngredientListEntity IngredientList { get; set; } = null!;
}