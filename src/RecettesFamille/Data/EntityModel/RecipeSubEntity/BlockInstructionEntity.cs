namespace RecettesFamille.Data.EntityModel.RecipeSubEntity;

public class BlockInstructionEntity : BlockBase
{
    public string Instruction { get; set; } = string.Empty;

    public BlockInstructionEntity()
    {
        HalfPage = false;
    }
}
