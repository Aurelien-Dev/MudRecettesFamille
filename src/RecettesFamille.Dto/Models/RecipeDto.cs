using RecettesFamille.Dto.Models.Blocks;

namespace RecettesFamille.Dto.Models;

public class RecipeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InformationPreparation { get; set; } = string.Empty;
    public int PrepTime { get; set; }
    public int CookingTime { get; set; }
    public int RestTime { get; set; }
    public int Portion { get; set; }
    public DateOnly CreatedDate { get; set; }
    public DateOnly? UpdatedDate { get; set; }
    public string Tags { get; set; } = string.Empty;

    public List<BlockBaseDto> BlocksInstructions { get; set; } = [];
}