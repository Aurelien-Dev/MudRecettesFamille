namespace RecettesFamille.Ai.ServicesNewVersion;

public class RecetteVector
{
    public string Key => RecipeId.ToString();
    public int RecipeId { get; set; }
    public string RecipeName { get; set; } = default!;
    public string? Tags { get; set; }
    public float[] Vector { get; set; } = default!;
}
