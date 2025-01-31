namespace RecettesFamille.Managers.AiGenerators.Models;

public class AiRecipe
{
    public required string Nom { get; set; }
    public required Preparation Preparation { get; set; }
    public required IngredientList[] Ingredients { get; set; }
    public required string[] Instructions { get; set; }
    public string[] Tags { get; set; }
}

public class Preparation
{
    public int TempsPreparation { get; set; }
    public int TempsCuisson { get; set; }
    public int Portions { get; set; }
}

public class IngredientList
{
    public required string NomListe { get; set; }
    public required Ingredient[] Ingredients { get; set; }
}

public class Ingredient
{
    public required string Nom { get; set; }
    public required string Quantite { get; set; }
}