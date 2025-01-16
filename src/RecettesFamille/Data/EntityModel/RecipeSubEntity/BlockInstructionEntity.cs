namespace RecettesFamille.Data.EntityModel.RecipeSubEntity;

public class BlockInstructionEntity : BlockBase
{
    public string Instruction { get; set; }

    public BlockInstructionEntity()
    {
        HalfPage = true;
    }
}
