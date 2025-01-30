namespace RecettesFamille.Data.EntityModel.Blocks;
public class BlockImageEntity : BlockBaseEntity
{
    public string Image { get; set; }

    public BlockImageEntity()
    {
        HalfPage = true;
        Image = "DALL·E.webp";
    }
}
