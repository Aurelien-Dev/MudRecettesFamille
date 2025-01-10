namespace RecettesFamille.Data.EntityModel.RecipeSubEntity;

public class BlockInstructionEntity : BlockBase
{
    public string Instruction { get; set; }

    public BlockInstructionEntity() { }
    public BlockInstructionEntity(string instruction) => Instruction = instruction;
}
