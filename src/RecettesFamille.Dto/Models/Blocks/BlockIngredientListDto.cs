using System.Text.Json.Serialization;

namespace RecettesFamille.Dto.Models.Blocks;

public class BlockIngredientListDto : BlockBaseDto
{
    public string Name { get; set; } = "Ingrédients";
    public int? Calories { get; set; }

    public List<IngredientDto> Ingredients { get; set; } = [];

    public BlockIngredientListDto()
    {
        HalfPage = true;
    }
}


public class IngredientDto()
{
    public int? Id { get; set; }
    public int Order { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public int IngredientListId { get; set; }

    [JsonIgnore]
    public BlockIngredientListDto IngredientList { get; set; } = null!;
}