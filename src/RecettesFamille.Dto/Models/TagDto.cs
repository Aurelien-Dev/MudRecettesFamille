namespace RecettesFamille.Dto.Models;

public class TagDto
{
    public int Id { get; set; }
    public string TagName { get; set; } = null!;
    public bool IsVisible { get; set; }
}
