namespace RecettesFamille.Dto.Models.Blocks;

public class BlockInstructionDto : BlockBaseDto
{
    public string Instruction { get; set; } = string.Empty;

    public BlockInstructionDto()
    {
        HalfPage = false;
    }
}
