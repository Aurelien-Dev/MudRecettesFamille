using RecettesFamille.Dto.Models.Blocks;

namespace RecettesFamille.Dto.Models;

public class RecipeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InformationPreparation { get; set; } = string.Empty;

    public List<BlockBaseDto> BlocksInstructions { get; set; } = new List<BlockBaseDto>();
}