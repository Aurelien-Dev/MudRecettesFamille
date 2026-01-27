using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using RecettesFamille.Data;
using RecettesFamille.Data.EntityModel.Blocks;
using System.Text;

namespace RecettesFamille.Ai.Services.Ingestion;

public class SQLRecipeSource(ApplicationDbContext dbContext) : IIngestionSource
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public string SourceId => nameof(SQLRecipeSource);

    public async Task<IEnumerable<IngestedDocument>> GetNewOrModifiedDocumentsAsync(IQueryable<IngestedDocument> existingDocuments, CancellationToken cancellationToken)
    {
        var results = new List<IngestedDocument>();
        var recipes = await _dbContext.Recipes.AsNoTracking().ToListAsync(cancellationToken);

        foreach (var recipe in recipes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var existingDocument = await existingDocuments
                .Where(d => d.SourceId == SourceId && d.Id == recipe.Id)
                .FirstOrDefaultAsync();

            if (existingDocument is null)
            {
                results.Add(new IngestedDocument
                {
                    Id = recipe.Id,
                    Version = FormatVersion(recipe.UpdatedDate),
                    SourceId = SourceId
                });
            }
            else if (existingDocument.Version != FormatVersion(recipe.UpdatedDate))
            {
                existingDocument.Version = FormatVersion(recipe.UpdatedDate);
                results.Add(existingDocument);
            }
        }

        return results;
    }

    public async Task<IEnumerable<IngestedDocument>> GetDeletedDocumentsAsync(IQueryable<IngestedDocument> existingDocuments, CancellationToken cancellationToken)
    {
        var recipeIds = await _dbContext.Recipes.Select(r => r.Id).ToListAsync();
        return await existingDocuments
            .Where(d => d.SourceId == SourceId && !recipeIds.Contains(d.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SemanticSearchRecord>> CreateRecordsForDocumentAsync(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, int documentId)
    {
        var recipe = await _dbContext.Recipes
                                     .Include(s => s.BlocksInstructions)
                                     .ThenInclude(b => ((BlockIngredientListEntity)b).Ingredients)
                                     .FirstOrDefaultAsync(s => s.Id == documentId);
        if (recipe == null) throw new FileNotFoundException($"Recipe with ID {documentId} not found.");

        var paragraphs = SplitIntoParagraphs(recipe.Name, recipe.BlocksInstructions);
        if (!paragraphs.Any())
            throw new EmptyRecipeException();

        var embeddings = await embeddingGenerator.GenerateAsync(paragraphs);

        return paragraphs.Zip(embeddings).Select((pair, index) => new SemanticSearchRecord
        {
            Key = $"{recipe.Id}_{index}",
            RecipeId = recipe.Id,
            RecipeName = recipe.Name,
            Tags = recipe.Tags,
            Ingredients = string.Join("; ", recipe.BlocksInstructions.OfType<BlockIngredientListEntity>().SelectMany(b => b.Ingredients.Select(i => $"{i.Name}: {i.Quantity}"))),
            Instructions = string.Join(Environment.NewLine, recipe.BlocksInstructions.OfType<BlockInstructionEntity>().Select(s => s.Instruction)),
            PrepTime = recipe.PrepTime,
            CookingTime = recipe.CookingTime,
            Portion = recipe.Portion,
            Vector = pair.Second.Vector
        });
    }

    private static IEnumerable<string> SplitIntoParagraphs(string recipeName, List<BlockBaseEntity> BlocksInstructions)
    {
        var paragraphs = new List<string>();

        paragraphs.Add($"---- Recipe name : {recipeName}");

        foreach (var item in BlocksInstructions.OfType<BlockIngredientListEntity>())
        {
            StringBuilder builder = new StringBuilder();
            var ingredientList = item.Ingredients.Select(s => $"{s.Name}:{s.Quantity}").ToList();
            if (!ingredientList.Any())
                continue;
            builder.AppendLine("---- Ingredient List");
            builder.AppendLine(string.Join(Environment.NewLine, ingredientList));
            paragraphs.Add(builder.ToString());
        }

        paragraphs.Add("---- Instructions");

        foreach (var item in BlocksInstructions.OfType<BlockInstructionEntity>())
        {
            if (string.IsNullOrEmpty(item.Instruction))
                continue;

            paragraphs.Add(item.Instruction);
        }

        return paragraphs;
    }

    private static string FormatVersion(DateOnly? updatedDate)
    {
        // Stable, culture-invariant version string; DateOnly precision matches your domain model.
        // If UpdatedDate is null, treat it as an empty version (will cause the first ingestion to create records).
        return updatedDate is null
            ? string.Empty
            : updatedDate.Value.ToString("O", System.Globalization.CultureInfo.InvariantCulture);
    }
}
