using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using RecettesFamille.Data;
using RecettesFamille.Data.EntityModel;
using RecettesFamille.Data.EntityModel.Blocks;
using System.Text;

namespace RecettesFamille.Ai.ServicesNewVersion;

internal class DbDataIngestor(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, ApplicationDbContext applicationDb)
{
    public async Task IngestData()
    {
        var folder = "vectors/recettes";

        var recipes = await applicationDb.Recipes
                             .Include(s => s.BlocksInstructions)
                             .ThenInclude(b => ((BlockIngredientListEntity)b).Ingredients).ToListAsync();

        foreach (RecipeEntity recette in recipes)
        {
            string instructions = SplitIntoParagraphs(recette.BlocksInstructions);
            
            var vec = await embeddingGenerator.GenerateAsync(instructions);

            var record = new RecetteVector
            {
                RecipeId = recette.Id,
                RecipeName = recette.Name,
                Tags = recette.Tags,
                Vector = vec.Vector.ToArray(),
            };

            VectorStorage.Save(folder, record);
        }

    }


    private static string SplitIntoParagraphs(List<BlockBaseEntity> BlocksInstructions)
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

        paragraphs.Add("---- Instructions List");
        foreach (var item in BlocksInstructions.OfType<BlockInstructionEntity>())
        {
            if (string.IsNullOrEmpty(item.Instruction))
                continue;

            paragraphs.Add(item.Instruction);
        }

        return string.Join(Environment.NewLine, paragraphs.ToArray());
    }

}
