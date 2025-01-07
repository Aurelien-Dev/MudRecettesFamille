using System.Text.Json.Serialization;

namespace RecettesFamille.Data.DtoModelInovate;

public class RecetteDto
{
    public string Name { get; set; }
    public string InformationPreparation { get; set; }
    public List<BlockBase> BlocksInstructions { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "__type")]
[JsonDerivedType(typeof(BlockInstruction))]
[JsonDerivedType(typeof(BlockImage))]
[JsonDerivedType(typeof(BlockIngredientList))]
public class BlockBase
{
    public int Order { get; set; }
}

public class BlockInstruction : BlockBase
{
    public string Instruction { get; set; }

    public BlockInstruction() { }
    public BlockInstruction(string instruction) => Instruction = instruction;
}

//Block image
public class BlockImage : BlockBase
{
    public string Image { get; set; }
}

//IngredientList
public class BlockIngredientList : BlockBase
{
    public List<IngredientDto> Ingredients { get; set; }
}

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
