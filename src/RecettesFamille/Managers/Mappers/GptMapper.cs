using RecettesFamille.Data.EntityModel.RecipeSubEntity;
using RecettesFamille.Data.EntityModel;
using RecettesFamille.Managers.Models;

namespace RecettesFamille.Managers.Mappers
{
    public static class GptMapper
    {
        public static RecipeEntity ConvertToRecipeEntity(GptRecipe gptRecipe)
        {
            if (gptRecipe == null)
            {
                throw new ArgumentNullException(nameof(gptRecipe));
            }

            var recipeEntity = new RecipeEntity
            {
                Name = gptRecipe.Nom,
                InformationPreparation = $"Temps de préparation: {gptRecipe.Preparation.TempsPreparation} minutes, Temps de cuisson: {gptRecipe.Preparation.TempsCuisson} minutes, Portions: {gptRecipe.Preparation.Portions}",
                BlocksInstructions = new List<BlockBase>()
            };

            // Convert ingredients to BlockIngredientListEntity
            var ingredientBlocks = gptRecipe.Ingredients.Select((ingredient, index) => new BlockIngredientListEntity
            {
                Order = index + 1,
                Name = ingredient.NomListe,
                HalfPage = true,
                Ingredients = ingredient.Ingredients.Select((ing, ingIndex) => new IngredientEntity
                {
                    Order = ingIndex + 1,
                    Name = ing.Nom,
                    Quantity = ing.Quantite
                }).ToList()
            }).ToList();
            recipeEntity.BlocksInstructions.AddRange(ingredientBlocks);

            // Convert instructions to a single BlockInstructionEntity
            var instructionBlock = new BlockInstructionEntity
            {
                Order = recipeEntity.BlocksInstructions.Count,
                Instruction = string.Join("\n\n", gptRecipe.Instructions),
                Recipe = null // Assuming the Recipe property will be set later
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
