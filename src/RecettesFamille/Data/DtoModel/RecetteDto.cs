namespace RecettesFamille.Data.DtoModel;

public record class RecetteDto
{
    public string Title { get; set; }
    public List<IngredientDto> Ingredients { get; set; }
    public List<DescriptionDto> Descriptions { get; set; }
}