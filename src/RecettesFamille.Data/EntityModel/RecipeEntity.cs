using System.ComponentModel.DataAnnotations;
using RecettesFamille.Data.EntityModel.Blocks;
using RecettesFamille.Rag.Ingestion;
using System.Security.Cryptography;
using System.Text;

namespace RecettesFamille.Data.EntityModel;

public class RecipeEntity : IIngestionEntity
{
    public int Id { get; set; }
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(50)]
    public string InformationPreparation { get; set; } = string.Empty;
    public int PrepTime { get; set; }
    public int CookingTime { get; set; }
    public int RestTime { get; set; }
    public int Portion { get; set; }
    public DateOnly CreatedDate { get; set; }
    public DateOnly? UpdatedDate { get; set; }

    [MaxLength(200)]
    public string Tags { get; set; } = string.Empty;

    public ICollection<BlockBaseEntity> BlocksInstructions { get; set; } = [];

    public ICollection<ApplicationUser> FavoritedByUsers { get; set; } = [];

    // IIngestionEntity implementation
    public string GetSearchableContent()
    {
        var ingredients = GetIngredientsText();
        var instructions = GetInstructionsText();

        return $"{Name}\n" +
               $"Catégorie: {Tags}\n" +
               $"Temps de préparation: {PrepTime} min\n" +
               $"Temps de cuisson: {CookingTime} min\n" +
               $"Temps de repos: {RestTime} min\n" +
               $"Portions: {Portion}\n" +
               $"Informations: {InformationPreparation}\n\n" +
               $"{ingredients}\n\n" +
               $"{instructions}";
    }

    public List<SearchableChunk> GetSearchableChunks()
    {
        var chunks = new List<SearchableChunk>
        {
            new("Metadata", $"{Name}\n" +
                           $"Catégorie: {Tags}\n" +
                           $"Temps de préparation: {PrepTime} min, Cuisson: {CookingTime} min, Repos: {RestTime} min\n" +
                           $"Portions: {Portion}\n" +
                           $"Informations: {InformationPreparation}"),
            new("Description", $"{Name}\n{InformationPreparation}"),
            new("Ingredients", $"Ingrédients pour {Name}:\n{GetIngredientsText()}"),
            new("Instructions", $"Préparation de {Name}:\n{GetInstructionsText()}")
        };

        return chunks;
    }

    public string GetCategory() => string.IsNullOrWhiteSpace(Tags) ? "Général" : Tags;

    public string CalculateContentHash()
    {
        var content = $"{Name}|{Tags}|{InformationPreparation}|{PrepTime}|{CookingTime}|{RestTime}|{Portion}|" +
                     $"{GetIngredientsText()}|{GetInstructionsText()}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToBase64String(hash);
    }

    // Helper methods
    private string GetIngredientsText()
    {
        var ingredientBlocks = BlocksInstructions
            .OfType<BlockIngredientListEntity>()
            .OrderBy(b => b.Order);

        var sb = new StringBuilder();
        foreach (var block in ingredientBlocks)
        {
            if (!string.IsNullOrWhiteSpace(block.Name))
            {
                sb.AppendLine(block.Name);
            }

            foreach (var ingredient in block.Ingredients.OrderBy(i => i.Order))
            {
                sb.AppendLine($"- {ingredient.Quantity} {ingredient.Name}".Trim());
            }

            if (block.Calories.HasValue)
            {
                sb.AppendLine($"(Calories: {block.Calories})");
            }

            sb.AppendLine();
        }

        return sb.ToString().Trim();
    }

    private string GetInstructionsText()
    {
        var instructionBlocks = BlocksInstructions
            .OfType<BlockInstructionEntity>()
            .OrderBy(b => b.Order);

        var sb = new StringBuilder();
        int step = 1;
        foreach (var block in instructionBlocks)
        {
            if (!string.IsNullOrWhiteSpace(block.Instruction))
            {
                sb.AppendLine($"{step}. {block.Instruction}");
                step++;
            }
        }

        return sb.ToString().Trim();
    }
}
