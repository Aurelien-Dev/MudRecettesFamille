using Microsoft.Extensions.VectorData;
using System.Text;

namespace RecettesFamille.Ai.Services;

public class SemanticSearchRecord
{
    [VectorStoreRecordKey]
    public required string Key { get; set; }

    [VectorStoreRecordData(IsFilterable = true)]
    public int RecipeId { get; set; }

    [VectorStoreRecordData(IsFilterable = true)]
    public required string RecipeName { get; set; } // Updated from FileName to RecipeName

    [VectorStoreRecordData]
    public required string Text { get; set; }

    [VectorStoreRecordData(IsFilterable = true)]
    public string? Tags { get; set; }

    [VectorStoreRecordData]
    public string? Ingredients { get; set; }

    [VectorStoreRecordData]
    public string? Instructions { get; set; } // Nouvelle propriété

    [VectorStoreRecordData]
    public int PrepTime { get; set; } // New field for preparation time

    [VectorStoreRecordData]
    public int CookingTime { get; set; } // New field for cooking time

    [VectorStoreRecordData]
    public int Portion { get; set; } // New field for portion size

    [VectorStoreRecordVector(1536, DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Key: {Key}");
        sb.AppendLine($"RecipeId: {RecipeId}");
        sb.AppendLine($"RecipeName: {RecipeName}");
        sb.AppendLine($"Text: {Text}");
        sb.AppendLine($"Tags: {Tags ?? "N/A"}");
        sb.AppendLine($"Ingredients: {Ingredients ?? "N/A"}");
        sb.AppendLine($"Instructions: {Instructions ?? "N/A"}");
        sb.AppendLine($"PrepTime: {PrepTime} minutes");
        sb.AppendLine($"CookingTime: {CookingTime} minutes");
        sb.AppendLine($"Portion: {Portion}");

        return sb.ToString();
    }

}
