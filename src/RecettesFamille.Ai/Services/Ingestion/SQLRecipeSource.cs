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

    public async Task<IEnumerable<IngestedDocument>> GetNewOrModifiedDocumentsAsync(IQueryable<IngestedDocument> existingDocuments)
    {
        var results = new List<IngestedDocument>();
        var recipes = await _dbContext.Recipes.AsNoTracking().ToListAsync();

        foreach (var recipe in recipes)
        {
            var existingDocument = await existingDocuments
                .Where(d => d.SourceId == SourceId && d.Id == recipe.Id.ToString())
                .FirstOrDefaultAsync();

            if (existingDocument is null)
            {
                results.Add(new IngestedDocument
                {
                    Id = recipe.Id.ToString(),
                    Version = recipe.UpdatedDate.ToString(),
                    SourceId = SourceId
                });
            }
            else if (existingDocument.Version != recipe.UpdatedDate.ToString())
            {
                existingDocument.Version = recipe.UpdatedDate.ToString();
                results.Add(existingDocument);
            }
        }

        return results;
    }

    public async Task<IEnumerable<IngestedDocument>> GetDeletedDocumentsAsync(IQueryable<IngestedDocument> existingDocuments)
    {
        var recipeIds = await _dbContext.Recipes.Select(r => r.Id.ToString()).ToListAsync();
        return await existingDocuments
            .Where(d => d.SourceId == SourceId && !recipeIds.Contains(d.Id))
            .ToListAsync();
    }

    public async Task<IEnumerable<SemanticSearchRecord>> CreateRecordsForDocumentAsync(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, string documentId)
    {
        var recipe = await _dbContext.Recipes
                                     .Include(s => s.BlocksInstructions)
                                     .ThenInclude(b => ((BlockIngredientListEntity)b).Ingredients)
                                     .FirstOrDefaultAsync(s => s.Id == int.Parse(documentId));
        if (recipe == null) throw new FileNotFoundException($"Recipe with ID {documentId} not found.");

        var paragraphs = SplitIntoParagraphs(recipe.BlocksInstructions);
        if (!paragraphs.Any())
            throw new EmptyRecipeException();

        var embeddings = await embeddingGenerator.GenerateAsync(paragraphs);

        return paragraphs.Zip(embeddings).Select((pair, index) => new SemanticSearchRecord
        {
            Key = $"{recipe.Id}_{index}",
            RecipeName = recipe.Name, // Updated property
            Text = pair.First,
            Tags = recipe.Tags,
            Ingredients = string.Join("; ", recipe.BlocksInstructions.OfType<BlockIngredientListEntity>().SelectMany(b => b.Ingredients.Select(i => $"{i.Name}: {i.Quantity}"))),
            Instructions = string.Join(Environment.NewLine, recipe.BlocksInstructions.OfType<BlockInstructionEntity>().Select(s => s.Instruction)),
            PrepTime = recipe.PrepTime, // New property
            CookingTime = recipe.CookingTime, // New property
            Portion = recipe.Portion, // New property
            Vector = pair.Second.Vector
        });
    }

    private static IEnumerable<string> SplitIntoParagraphs(List<BlockBaseEntity> BlocksInstructions)
    {
        var paragraphs = new List<string>();
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

        foreach (var item in BlocksInstructions.OfType<BlockInstructionEntity>())
        {
            if (string.IsNullOrEmpty(item.Instruction))
                continue;

            paragraphs.Add(item.Instruction);
        }

        return paragraphs;
    }
}
