using RecettesFamille.Data.DtoModelInovate;

namespace RecettesFamille.Components.RecetteBook;

public class SampleData
{
    public static List<RecetteDto> GetSampleRecettes()
    {
        return new List<RecetteDto>
        {
            new RecetteDto
            {
                Name = "Chocolate Cake",
                BlocksInstructions = new List<BlockBase>
                {
                    new BlockImage
                    {
                        Order = 0,
                        Image = "https://static.750g.com/images/1200-675/9823eb627203c878f3e36d72f8ce6d1c/tarte-aux-pommes.jpg"
                    },
                    new BlockIngredientList
                    {
                        Order = 1,
                        Ingredients = new List<IngredientDto>
                        {
                            new IngredientDto("Flour", IngredientType.Ingredient)
                            {
                                Order = 1,
                                Quantity = "2 cups",
                                Identifier = "1"
                            },
                            new IngredientDto("Sugar", IngredientType.Ingredient)
                            {
                                Order = 2,
                                Quantity = "2 cups",
                                Identifier = "2"
                            },
                            new IngredientDto("Cocoa powder", IngredientType.Ingredient)
                            {
                                Order = 3,
                                Quantity = "3/4 cup",
                                Identifier = "3"
                            },
                            new IngredientDto("Baking powder", IngredientType.Ingredient)
                            {
                                Order = 4,
                                Quantity = "1 1/2 tsp",
                                Identifier = "4"
                            },
                            new IngredientDto("Salt", IngredientType.Ingredient)
                            {
                                Order = 5,
                                Quantity = "1 tsp",
                                Identifier = "5"
                            }
                        }
                    },
                    new BlockIngredientList
                    {
                        Order = 2,
                        Ingredients = new List<IngredientDto>
                        {
                            new IngredientDto("Eggs", IngredientType.Ingredient)
                            {
                                Order = 6,
                                Quantity = "2",
                                Identifier = "6"
                            },
                            new IngredientDto("Milk", IngredientType.Ingredient)
                            {
                                Order = 7,
                                Quantity = "1 cup",
                                Identifier = "7"
                            },
                            new IngredientDto("Vegetable oil", IngredientType.Ingredient)
                            {
                                Order = 8,
                                Quantity = "1/2 cup",
                                Identifier = "8"
                            },
                            new IngredientDto("Vanilla extract", IngredientType.Ingredient)
                            {
                                Order = 9,
                                Quantity = "2 tsp",
                                Identifier = "9"
                            },
                            new IngredientDto("Boiling water", IngredientType.Ingredient)
                            {
                                Order = 10,
                                Quantity = "1 cup",
                                Identifier = "10"
                            }
                        }
                    },
                    new BlockInstruction("Preheat the oven to 350°F (175°C). Grease and flour two nine-inch round pans.")
                    {
                        Order = 3
                    },
                    new BlockInstruction("Bake for 30 to 35 minutes in the preheated oven, until the cake tests done with a toothpick.")
                    {
                        Order = 4
                    }
                }
            }
        };
    }
}
