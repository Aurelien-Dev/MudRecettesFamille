namespace RecettesFamille.Data.DtoModel;

public enum IngredientType
{
    Ingredient,
    Divider
}

public record class IngredientDto(string Name, IngredientType Type)
{
    public int Order { get; set; }
    public string Name { get; set; } = Name;
    public string Quantity { get; set; } = string.Empty;
    public IngredientType Type { get; set; } = Type;
    public string Identifier { get; set; } = "0";
}