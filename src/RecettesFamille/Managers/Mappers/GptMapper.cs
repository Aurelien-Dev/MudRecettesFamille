using RecettesFamille.Dto.Models;
using RecettesFamille.Dto.Models.Blocks;
using RecettesFamille.Managers.AiGenerators.Models;

namespace RecettesFamille.Managers.Mappers
{
    public static class GptMapper
    {
        public static RecipeDto ConvertToRecipeDto(AiRecipe gptRecipe)
        {
            if (gptRecipe == null)
            {
                throw new ArgumentNullException(nameof(gptRecipe));
            }

            var recipe = new RecipeDto
            {
                Name = gptRecipe.Nom,
                PrepTime = gptRecipe.Preparation.TempsPreparation,
                CookingTime = gptRecipe.Preparation.TempsCuisson,
                Portion = gptRecipe.Preparation.Portions,
                Tags = string.Join("|", gptRecipe.Tags),
                BlocksInstructions = new List<BlockBaseDto>()
            };

            // Convert ingredients to BlockIngredientListEntity
            var ingredientBlocks = gptRecipe.Ingredients.Select((ingredient, index) => new BlockIngredientListDto
            {
                Order = index,
                Name = ingredient.NomListe,
                HalfPage = true,
                Ingredients = ingredient.Ingredients.Select((ing, ingIndex) => new IngredientDto
                {
                    Order = ingIndex,
                    Name = ing.Nom,
                    Quantity = ing.Quantite
                }).ToList()
            }).ToList();
            recipe.BlocksInstructions.AddRange(ingredientBlocks);

            // Convert instructions to a single BlockInstructionEntity
            var instructionBlock = new BlockInstructionDto
            {
                Order = recipe.BlocksInstructions.Count,
                Instruction = string.Join("\n\n", gptRecipe.Instructions)
            };
            recipe.BlocksInstructions.Add(instructionBlock);

            // Set the Recipe property for each block
            foreach (var block in recipe.BlocksInstructions)
            {
                block.Recipe = recipe;
            }

            return recipe;
        }
    }
}
