namespace RecettesFamille.Managers.AiGenerators.Models;

/// <summary>
/// Résultat structuré de la génération IA d'un résumé YouTube incluant les métadonnées détectées.
/// </summary>
public sealed class AiSummaryGenerationResult
{
    /// <summary>
    /// Le résumé Markdown de la vidéo YouTube.
    /// </summary>
    public required string Summary { get; init; }

    /// <summary>
    /// Le pays principal détecté dans la vidéo (nullable si non identifiable).
    /// </summary>
    public DetectedCountryResult? MainCountry { get; init; }

    /// <summary>
    /// Liste des catégories détectées parmi les catégories existantes.
    /// </summary>
    public List<DetectedCategoryResult> Categories { get; init; } = [];
}

/// <summary>
/// Représente un pays détecté par l'IA avec son score de confiance.
/// </summary>
public sealed class DetectedCountryResult
{
    /// <summary>
    /// Nom du pays en français.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Code ISO 3166-1 alpha-2 du pays (ex: "FR", "JP", "US").
    /// Nullable si l'IA n'est pas certaine du code.
    /// </summary>
    public string? IsoCode { get; init; }

    /// <summary>
    /// Score de confiance entre 0 et 1.
    /// </summary>
    public double Confidence { get; init; }
}

/// <summary>
/// Représente une catégorie détectée par l'IA avec son score de confiance.
/// </summary>
public sealed class DetectedCategoryResult
{
    /// <summary>
    /// Nom exact de la catégorie (doit correspondre à une catégorie existante).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Score de confiance entre 0 et 1.
    /// </summary>
    public double Confidence { get; init; }
}
