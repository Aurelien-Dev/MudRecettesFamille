using RecettesFamille.Data.EntityModel.Blocks;
using RecettesFamille.Data.EntityModel;
using RecettesFamille.Managers.AiGenerators.Models;
using RecettesFamille.Dto.Models;
using RecettesFamille.Dto.Models.Blocks;

namespace RecettesFamille.Managers.Mappers
{
    public static class GptMapper
    {
        public static RecipeDto ConvertToRecipeEntity(AiRecipe gptRecipe)
        {
            if (gptRecipe == null)
            {
                throw new ArgumentNullException(nameof(gptRecipe));
            }

            var recipeEntity = new RecipeDto
            {
                Name = gptRecipe.Nom,
                InformationPreparation = $"Temps de préparation: {gptRecipe.Preparation.TempsPreparation} minutes, Temps de cuisson: {gptRecipe.Preparation.TempsCuisson} minutes, Portions: {gptRecipe.Preparation.Portions}",
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
            recipeEntity.BlocksInstructions.AddRange(ingredientBlocks);

            // Convert instructions to a single BlockInstructionEntity
            var instructionBlock = new BlockInstructionDto
            {
                Order = recipeEntity.BlocksInstructions.Count,
                Instruction = string.Join("\n\n", gptRecipe.Instructions)
            };
            recipeEntity.BlocksInstructions.Add(instructionBlock);

            // Set the Recipe property for each block
            foreach (var block in recipeEntity.BlocksInstructions)
            {
                block.Recipe = recipeEntity;
            }

            return recipeEntity;
        }
    }
}
