namespace RecettesFamille.Ai.Services;

public class SemanticSearchRecord
{
    public required string Key { get; set; }
    public int RecipeId { get; set; }
    public required string RecipeName { get; set; }
    public string? Tags { get; set; }
    public string? Ingredients { get; set; }
    public string? Instructions { get; set; }
    public int PrepTime { get; set; }
    public int CookingTime { get; set; }
    public int Portion { get; set; }
    public ReadOnlyMemory<float> Vector { get; set; }
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Key: {Key}");
        sb.AppendLine($"RecipeId: {RecipeId}");
        sb.AppendLine($"RecipeName: {RecipeName}");
        sb.AppendLine($"Tags: {Tags ?? "N/A"}");
        sb.AppendLine($"Ingredients: {Ingredients ?? "N/A"}");
        sb.AppendLine($"Instructions: {Instructions ?? "N/A"}");
        sb.AppendLine($"PrepTime: {PrepTime} minutes");
        sb.AppendLine($"CookingTime: {CookingTime} minutes");
        sb.AppendLine($"Portion: {Portion}");
        return sb.ToString();
    }
}
