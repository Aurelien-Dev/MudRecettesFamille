namespace RecettesFamille.Data.EntityModel.RecipeSubEntity;
public class BlockImageEntity : BlockBase
{
    public string Image { get; set; }

    public BlockImageEntity()
    {
        HalfPage = true;
        Image = "DALL·E.webp";
    }
}
