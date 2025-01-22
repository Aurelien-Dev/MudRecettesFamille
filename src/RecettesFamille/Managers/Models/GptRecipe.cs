namespace RecettesFamille.Managers.Models;

public class GptRecipe
{
    public string Nom { get; set; }
    public Preparation Preparation { get; set; }
    public Ingredient[] Ingredients { get; set; }
    public string[] Instructions { get; set; }
}

public class Preparation
{
    public int TempsPreparation { get; set; }
    public int TempsCuisson { get; set; }
    public int Portions { get; set; }
}

public class Ingredient
{
    public string NomListe { get; set; }
    public Ingredient1[] Ingredients { get; set; }
}

public class Ingredient1
{
    public string Nom { get; set; }
    public string Quantite { get; set; }
}
