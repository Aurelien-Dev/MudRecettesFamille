namespace RecettesFamille.Dto.Models.Blocks;

public class BlockImageDto : BlockBaseDto
{
    public string Image { get; set; }

    public BlockImageDto()
    {
        HalfPage = true;
        Image = "DALL·E.webp";
    }
}
