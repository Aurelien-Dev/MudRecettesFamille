namespace RecettesFamille.Dto.Models.Blocks;

public class BlockImageDto : BlockBaseDto
{
    public byte[]? Image { get; set; }

    public BlockImageDto()
    {
        HalfPage = true;
    }
}
