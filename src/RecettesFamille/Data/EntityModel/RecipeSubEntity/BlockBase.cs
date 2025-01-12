using System.ComponentModel.DataAnnotations;

namespace RecettesFamille.Data.EntityModel.RecipeSubEntity;
public abstract class BlockBase
{
    public int? Id { get; set; }
    public int Order { get; set; }

    public RecipeEntity Recipe { get; set; }
}
