﻿using System.Text.Json.Serialization;

namespace RecettesFamille.Data.EntityModel.Blocks;
public class BlockIngredientListEntity : BlockBaseEntity
{
    public string Name { get; set; } = "Ingrédients";
    public List<IngredientEntity> Ingredients { get; set; } = new List<IngredientEntity>();

    public BlockIngredientListEntity()
    {
        HalfPage = true;
    }
}


public class IngredientEntity()
{
    public int? Id { get; set; }
    public int Order { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public int IngredientListId { get; set; }

    [JsonIgnore]
    public BlockIngredientListEntity IngredientList { get; set; } = null!;
}