namespace RecettesFamille.Data.EntityModel;

public class RecetteEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string InformationPreparation { get; set; }

    public List<BlockBase> BlocksInstructions { get; set; }
}

public abstract class BlockBase
{
    public int Id { get; set; }
    public int Order { get; set; }
}

public class BlockInstruction : BlockBase
{
    public string Instruction { get; set; }

    public BlockInstruction() { }
    public BlockInstruction(string instruction) => Instruction = instruction;
}

public class BlockImage : BlockBase
{
    public string Image { get; set; }
}

public class BlockIngredientList : BlockBase
{
    public List<IngredientDto> Ingredients { get; set; }
}

public record class IngredientDto()
{
    public int Id { get; set; }
    public int Order { get; set; }
    public string Name { get; set; }
    public string Quantity { get; set; } = string.Empty;
}
